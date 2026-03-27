using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Commands.MarkOrderFailed;

public interface IMarkOrderFailedCommandHandler : ICommandHandler<MarkOrderFailedCommand, OrderResponse> { }
