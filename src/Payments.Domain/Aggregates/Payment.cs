using Payments.Domain.Enums;
using Payments.Domain.Events;
using SharedKernel;

namespace Payments.Domain.Aggregates;

public sealed class Payment : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid IdempotencyKey { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "AUD";
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    private readonly List<PaymentAttempt> _attempts = new();
    public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    private Payment() { }

    public static Payment Create(Guid orderId, Guid customerId, decimal amount, string currency, Guid idempotencyKey)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            IdempotencyKey = idempotencyKey,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Authorise(string gatewayRef)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot authorise payment in '{Status}' state.");

        Status = PaymentStatus.Authorised;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        _attempts.Add(new PaymentAttempt(Id, "Authorise", true, gatewayRef));
        RaiseDomainEvent(new PaymentAuthorisedEvent(Id, OrderId, Amount));
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorised)
            throw new InvalidOperationException($"Cannot capture payment in '{Status}' state.");

        Status = PaymentStatus.Captured;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        _attempts.Add(new PaymentAttempt(Id, "Capture", true, null));
        RaiseDomainEvent(new PaymentCapturedEvent(Id, OrderId, Amount));
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        _attempts.Add(new PaymentAttempt(Id, "Fail", false, reason));
        RaiseDomainEvent(new PaymentFailedEvent(Id, OrderId, reason));
    }

    public void Refund(decimal amount, string reason)
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException($"Cannot refund payment in '{Status}' state.");
        if (amount > Amount)
            throw new InvalidOperationException("Refund amount exceeds payment amount.");

        Status = PaymentStatus.Refunded;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        _attempts.Add(new PaymentAttempt(Id, "Refund", true, reason));
        RaiseDomainEvent(new PaymentRefundedEvent(Id, OrderId, amount, reason));
    }
}

public sealed class PaymentAttempt
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public string Action { get; private set; } = null!;
    public bool Success { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }

    private PaymentAttempt() { }

    internal PaymentAttempt(Guid paymentId, string action, bool success, string? gatewayResponse)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Action = action;
        Success = success;
        GatewayResponse = gatewayResponse;
        AttemptedAt = DateTimeOffset.UtcNow;
    }
}
