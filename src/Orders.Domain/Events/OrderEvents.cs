using Orders.Domain.Enums;
using SharedKernel;

namespace Orders.Domain.Events;

public sealed record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency) : DomainEvent;

public sealed record OrderConfirmedEvent(Guid OrderId) : DomainEvent;

public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : DomainEvent;

public sealed record OrderFailedEvent(Guid OrderId, string FailureReason) : DomainEvent;
