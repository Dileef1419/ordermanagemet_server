using FluentValidation;

namespace Fulfilment.Application.Commands.CreateShipment;

public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("WarehouseId must be greater than zero.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Shipment must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Sku)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(50);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
