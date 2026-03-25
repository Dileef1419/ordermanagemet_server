using Payments.Application.DTOs;
using SharedKernel;

namespace Payments.Application.Commands.AuthorisePayment;

public interface IAuthorisePaymentCommandHandler : ICommandHandler<AuthorisePaymentCommand, PaymentResponse> { }
