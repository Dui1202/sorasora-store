using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateProductVariantRequestValidator
    : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator(
        IValidator<CreateProductVariantImageRequest> imageValidator
    )
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductVariant.MaxNameLength);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProductVariantImage).NotNull().SetValidator(imageValidator);
    }
}
