using Payments.Application.DTOs;
using SharedKernel;

namespace Payments.Application.Commands.RefundPayment;

public interface IRefundPaymentCommandHandler : ICommandHandler<RefundPaymentCommand, PaymentResponse> { }
