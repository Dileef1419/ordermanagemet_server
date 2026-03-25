using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Commands.CancelOrder;

public interface ICancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, OrderResponse> { }
