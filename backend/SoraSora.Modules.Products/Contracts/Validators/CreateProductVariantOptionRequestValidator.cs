using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateProductVariantOptionRequestValidator
    : AbstractValidator<CreateProductVariantOptionRequest>
{
    public CreateProductVariantOptionRequestValidator()
    {
        RuleFor(x => x.VariationOptionClientId).NotEmpty();
    }
}
