using Orders.Application.DTOs;

namespace Orders.Application.Commands.ShipOrder;

public interface IShipOrderCommandHandler
{
    Task<OrderResponse> Handle(ShipOrderCommand cmd, CancellationToken ct);
}
