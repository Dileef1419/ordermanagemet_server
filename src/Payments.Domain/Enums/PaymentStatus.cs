namespace Payments.Domain.Enums;

public enum PaymentStatus
{
    Pending,
    Authorised,
    Captured,
    Failed,
    Refunded
}
