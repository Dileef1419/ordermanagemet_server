using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Commands.MarkOrderFailed;
using Orders.Application.DTOs;
using Orders.Domain.Exceptions;
using Orders.Infrastructure.Persistence;
using SharedKernel;

namespace Orders.Infrastructure.CommandHandlers;

public class MarkOrderFailedHandler : IMarkOrderFailedCommandHandler
{
    private readonly OrdersDbContext _db;

    public MarkOrderFailedHandler(OrdersDbContext db) => _db = db;

    public async Task<OrderResponse> Handle(MarkOrderFailedCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new OrderNotFoundException(cmd.OrderId);

        order.MarkFailed(cmd.Reason);

        foreach (var evt in order.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                AggregateId = order.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        await _db.SaveChangesAsync(ct);
        order.ClearDomainEvents();

        return new OrderResponse(order.Id, order.Status.ToString());
    }
}
