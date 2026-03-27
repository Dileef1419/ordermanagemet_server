namespace Payments.Application.Commands.AuthorisePayment;

public record AuthorisePaymentCommand(
    Guid IdempotencyKey,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);
