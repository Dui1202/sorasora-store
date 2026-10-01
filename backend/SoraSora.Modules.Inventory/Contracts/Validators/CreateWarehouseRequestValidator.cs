using FluentValidation;
using SoraSora.Modules.Inventory.Contracts.Requests;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Contracts.Validators;

public sealed class CreateWarehouseRequestValidator : AbstractValidator<CreateWarehouseRequest>
{
    public CreateWarehouseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Warehouse.MaxNameLength);
    }
}
