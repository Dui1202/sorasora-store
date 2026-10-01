using FluentValidation;
using SoraSora.Modules.Inventory.Contracts.Requests;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Contracts.Validators;

public sealed class UpdateWarehouseRequestValidator : AbstractValidator<UpdateWarehouseRequest>
{
    public UpdateWarehouseRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Warehouse.MaxNameLength);
    }
}
