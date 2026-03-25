namespace Payments.Application.Commands.RefundPayment;

public record RefundPaymentCommand(Guid PaymentId, decimal Amount, string Reason);
