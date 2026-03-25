using FluentValidation;

namespace Fulfilment.Application.Commands.DispatchShipment;

public class DispatchShipmentCommandValidator : AbstractValidator<DispatchShipmentCommand>
{
    public DispatchShipmentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required.");

        RuleFor(x => x.CarrierRef)
            .NotEmpty().WithMessage("CarrierRef is required.")
            .MaximumLength(100);

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required.")
            .MaximumLength(100);
    }
}
