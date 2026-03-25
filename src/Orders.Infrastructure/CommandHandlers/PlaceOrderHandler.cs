using System.Text.Json;
using Orders.Application.Commands.PlaceOrder;
using Orders.Application.DTOs;
using Orders.Domain.Aggregates;
using Orders.Infrastructure.Persistence;
using SharedKernel;

namespace Orders.Infrastructure.CommandHandlers;

/// <summary>
/// Handles PlaceOrderCommand — writes aggregate + outbox + idempotency
/// in a single EF Core transaction (atomic consistency).
/// </summary>
public class PlaceOrderHandler : IPlaceOrderCommandHandler
{
    private readonly OrdersDbContext _db;

    public PlaceOrderHandler(OrdersDbContext db) => _db = db;

    public async Task<OrderResponse> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        // 1. Idempotency check
        var existing = await _db.ProcessedCommands.FindAsync(new object[] { cmd.IdempotencyKey }, ct);
        if (existing is not null)
            return JsonSerializer.Deserialize<OrderResponse>(existing.ResultPayload!)!;

        // 2. Map command to domain input
        var lineInputs = cmd.Lines
            .Select(l => new OrderLineInput(l.Sku, l.Quantity, l.UnitPrice))
            .ToList();

        // 3. Domain logic
        var order = Order.Place(cmd.CustomerId, cmd.CustomerName, lineInputs);
        await _db.Orders.AddAsync(order, ct);

        // 4. Write domain events to Outbox (same transaction)
        foreach (var evt in order.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                EventId = Guid.NewGuid(),
                AggregateId = order.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        // 5. Record idempotency
        var result = new OrderResponse(order.Id, order.Status.ToString());
        _db.ProcessedCommands.Add(new ProcessedCommand
        {
            IdempotencyKey = cmd.IdempotencyKey,
            CommandType = nameof(PlaceOrderCommand),
            ResultPayload = JsonSerializer.Serialize(result),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });

        // 6. Atomic save — aggregate + outbox + idempotency in one transaction
        await _db.SaveChangesAsync(ct);
        order.ClearDomainEvents();

        return result;
    }
}
