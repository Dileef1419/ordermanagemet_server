using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Commands.ConfirmOrder;

public interface IConfirmOrderCommandHandler : ICommandHandler<ConfirmOrderCommand, OrderResponse> { }
