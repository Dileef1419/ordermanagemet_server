using Orders.Application.DTOs;

namespace Orders.Application.Commands.DeliverOrder;

public interface IDeliverOrderCommandHandler
{
    Task<OrderResponse> Handle(DeliverOrderCommand cmd, CancellationToken ct);
}
