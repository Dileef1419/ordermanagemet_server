using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Application.Commands.ConfirmOrder;
using Orders.Application.DTOs;
using Orders.Domain.Exceptions;
using Orders.Infrastructure.Persistence;
using SharedKernel;

namespace Orders.Infrastructure.CommandHandlers;

public class ConfirmOrderHandler : IConfirmOrderCommandHandler
{
    private readonly OrdersDbContext _db;

    public ConfirmOrderHandler(OrdersDbContext db) => _db = db;

    public async Task<OrderResponse> Handle(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct)
            ?? throw new OrderNotFoundException(cmd.OrderId);

        order.Confirm();

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
