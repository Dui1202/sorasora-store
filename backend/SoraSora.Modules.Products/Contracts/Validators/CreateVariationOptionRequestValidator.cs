using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateVariationOptionRequestValidator
    : AbstractValidator<CreateVariationOptionRequest>
{
    public CreateVariationOptionRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
