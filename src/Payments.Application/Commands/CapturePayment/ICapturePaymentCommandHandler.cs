using Payments.Application.DTOs;
using SharedKernel;

namespace Payments.Application.Commands.CapturePayment;

public interface ICapturePaymentCommandHandler : ICommandHandler<CapturePaymentCommand, PaymentResponse> { }
