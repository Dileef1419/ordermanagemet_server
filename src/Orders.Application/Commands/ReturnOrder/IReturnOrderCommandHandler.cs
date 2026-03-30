using Orders.Application.DTOs;

namespace Orders.Application.Commands.ReturnOrder;

public interface IReturnOrderCommandHandler
{
    Task<OrderResponse> Handle(ReturnOrderCommand cmd, CancellationToken ct);
}
