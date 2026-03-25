using SharedKernel;

namespace Payments.Domain.Events;

public sealed record PaymentAuthorisedEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;
public sealed record PaymentCapturedEvent(Guid PaymentId, Guid OrderId, decimal Amount) : DomainEvent;
public sealed record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;
public sealed record PaymentRefundedEvent(Guid PaymentId, Guid OrderId, decimal Amount, string Reason) : DomainEvent;
