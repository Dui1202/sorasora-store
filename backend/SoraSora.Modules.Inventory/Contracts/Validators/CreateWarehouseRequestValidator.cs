using FluentValidation;
using SoraSora.Modules.Inventory.Domain;

public sealed class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Warehouse.MaxNameLength);
    }
}
