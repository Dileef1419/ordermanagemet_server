using Fulfilment.Application.DTOs;
using SharedKernel;

namespace Fulfilment.Application.Commands.DispatchShipment;

public interface IDispatchShipmentCommandHandler : ICommandHandler<DispatchShipmentCommand, ShipmentResponse> { }
