using SharedKernel;

namespace Fulfilment.Domain.Events;

public sealed record ShipmentCreatedEvent(Guid ShipmentId, Guid OrderId) : DomainEvent;
public sealed record ShipmentDispatchedEvent(Guid ShipmentId, string TrackingNumber, string CarrierRef) : DomainEvent;
public sealed record ShipmentDeliveredEvent(Guid ShipmentId, DateTimeOffset DeliveredAt) : DomainEvent;
public sealed record ShipmentExceptionEvent(Guid ShipmentId, string ExceptionType, string Details) : DomainEvent;
