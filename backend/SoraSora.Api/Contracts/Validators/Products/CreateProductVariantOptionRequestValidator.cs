using FluentValidation;
using SoraSora.Api.Contracts.Requests.Products.Create;

namespace SoraSora.Api.Contracts.Validators.Products;

public sealed class CreateProductVariantOptionRequestValidator
    : AbstractValidator<CreateProductVariantOptionRequest>
{
    public CreateProductVariantOptionRequestValidator()
    {
        RuleFor(x => x.VariationClientId).NotEmpty();
        RuleFor(x => x.VariationOptionClientId).NotEmpty();
    }
}
