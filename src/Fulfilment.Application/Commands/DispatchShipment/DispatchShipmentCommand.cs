namespace Fulfilment.Application.Commands.DispatchShipment;

public record DispatchShipmentCommand(Guid ShipmentId, string CarrierRef, string TrackingNumber);
