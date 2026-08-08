using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateProductVariantRequestValidator
    : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator(
        IValidator<CreateProductVariantImageRequest> imageValidator,
        IValidator<CreateProductVariantOptionRequest> variantOptionValidator
    )
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductVariant.MaxNameLength);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProductVariantImage).NotNull().SetValidator(imageValidator);
        RuleFor(x => x.ProductVariantOptions)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(HaveUniqueVariationClientIds)
            .WithMessage("Product variant variation client ids must be unique.")
            .Must(HaveUniqueVariationOptionClientIds)
            .WithMessage("Product variant option client ids must be unique.");
        RuleForEach(x => x.ProductVariantOptions).SetValidator(variantOptionValidator);
    }

    private static bool HaveUniqueVariationClientIds(
        IReadOnlyCollection<CreateProductVariantOptionRequest> options
    )
    {
        if (options.Any(x => string.IsNullOrWhiteSpace(x.VariationClientId)))
        {
            return true;
        }

        return options
                .Select(x => x.VariationClientId)
                .Distinct(StringComparer.Ordinal)
                .Count()
            == options.Count;
    }

    private static bool HaveUniqueVariationOptionClientIds(
        IReadOnlyCollection<CreateProductVariantOptionRequest> options
    )
    {
        if (options.Any(x => string.IsNullOrWhiteSpace(x.VariationOptionClientId)))
        {
            return true;
        }

        return options
                .Select(x => x.VariationOptionClientId)
                .Distinct(StringComparer.Ordinal)
                .Count()
            == options.Count;
    }
}
