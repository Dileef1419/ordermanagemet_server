using Fulfilment.Application.DTOs;
using SharedKernel;

namespace Fulfilment.Application.Commands.CreateShipment;

public interface ICreateShipmentCommandHandler : ICommandHandler<CreateShipmentCommand, ShipmentResponse> { }
