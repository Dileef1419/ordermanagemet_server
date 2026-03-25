using FluentValidation;

namespace Payments.Application.Commands.AuthorisePayment;

public class AuthorisePaymentCommandValidator : AbstractValidator<AuthorisePaymentCommand>
{
    public AuthorisePaymentCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}
