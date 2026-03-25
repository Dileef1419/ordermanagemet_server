namespace Payments.Application.Commands.AuthorisePayment;

public record AuthorisePaymentCommand(
    Guid IdempotencyKey,
    Guid OrderId,
    decimal Amount,
    string Currency);
