using FluentValidation;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator(
        IValidator<CreateProductImageRequest> createProductImageRequestValidator,
        IValidator<CreateProductVariantRequest> createProductVariantRequestValidator,
        IValidator<CreateVariationRequest> createVariationRequestValidator
    )
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength);
        RuleFor(x => x.SupplierCountry).NotNull().IsInEnum();
        RuleFor(x => x.SellMode).IsInEnum();
        RuleFor(x => x.IsPublished).NotNull();
        RuleFor(x => x.ProductImages)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(images => images.Count <= ProductUploadLimits.MaxProductImages)
            .WithMessage(
                $"A product can contain at most {ProductUploadLimits.MaxProductImages} images."
            )
            .Must(HaveUniquePositions);
        RuleForEach(x => x.ProductImages).SetValidator(createProductImageRequestValidator);
        RuleForEach(x => x.ProductVariants).SetValidator(createProductVariantRequestValidator);
        RuleForEach(x => x.Variations).SetValidator(createVariationRequestValidator);
        RuleFor(x => x.HasVariation).NotNull();

        When(
            request => !request.HasVariation,
            () =>
            {
                RuleFor(request => request.BaseCostPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.BaseSellingPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.ProductVariants).Empty();
                RuleFor(request => request.Variations).Empty();
            }
        );

        When(
            request => request.HasVariation,
            () =>
            {
                RuleFor(request => request.ProductVariants)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .Must(productVariants =>
                        productVariants.Count <= ProductUploadLimits.MaxProductVariants
                    )
                    .WithMessage(
                        $"A product can contain at most {ProductUploadLimits.MaxProductVariants} variants."
                    )
                    .Must(HaveUniqueVariantPositions)
                    .WithMessage("Product variant positions must be unique.")
                    .Must((request, variants) => HaveValidVariantOptionSelections(request, variants))
                    .WithMessage(
                        "Each product variant must select exactly one valid option for each variation."
                    )
                    .Must((request, variants) => HaveUniqueVariantOptionCombinations(request, variants))
                    .WithMessage("Product variant option combinations must be unique.");

                RuleFor(request => request.Variations)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .Must(HaveUniqueVariationPositions)
                    .WithMessage("Variation positions must be unique.")
                    .Must(HaveUniqueVariationNames)
                    .WithMessage("Variation names must be unique.")
                    .Must(HaveGloballyUniqueVariationOptionClientIds)
                    .WithMessage("Variation option client ids must be unique across the product.");
            }
        );
    }

    private static bool HaveUniquePositions(IReadOnlyCollection<CreateProductImageRequest> images)
    {
        return images.Select(x => x.Position).Distinct().Count() == images.Count;
    }

    private static bool HaveUniqueVariantPositions(
        IReadOnlyCollection<CreateProductVariantRequest> variants
    )
    {
        return variants.Select(x => x.Position).Distinct().Count() == variants.Count;
    }

    private static bool HaveUniqueVariationPositions(
        IReadOnlyCollection<CreateVariationRequest> variations
    )
    {
        return variations.Select(x => x.Position).Distinct().Count() == variations.Count;
    }

    private static bool HaveUniqueVariationNames(
        IReadOnlyCollection<CreateVariationRequest> variations
    )
    {
        if (variations.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return true;
        }

        return variations
                .Select(x => x.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
            == variations.Count;
    }

    private static bool HaveGloballyUniqueVariationOptionClientIds(
        IReadOnlyCollection<CreateVariationRequest> variations
    )
    {
        if (
            variations.Any(variation =>
                variation.VariationOptions is null
                || variation.VariationOptions.Any(option =>
                    string.IsNullOrWhiteSpace(option.ClientId)
                )
            )
        )
        {
            return true;
        }

        var optionClientIds = variations.SelectMany(x =>
            x.VariationOptions.Select(option => option.ClientId)
        );

        return optionClientIds.Distinct(StringComparer.Ordinal).Count()
            == variations.Sum(x => x.VariationOptions.Count);
    }

    private static bool HaveValidVariantOptionSelections(
        CreateProductRequest request,
        IReadOnlyCollection<CreateProductVariantRequest> variants
    )
    {
        if (
            request.Variations is null
            || !TryBuildVariationIndexByOptionClientId(
                request.Variations,
                out var variationByOption
            )
        )
        {
            return false;
        }

        foreach (var variant in variants)
        {
            if (
                variant.ProductVariantOptions is null
                || variant.ProductVariantOptions.Count != request.Variations.Count
            )
            {
                return false;
            }

            var selectedVariations = new HashSet<int>();

            foreach (var selectedOption in variant.ProductVariantOptions)
            {
                if (
                    !variationByOption.TryGetValue(
                        selectedOption.VariationOptionClientId,
                        out var variationIndex
                    )
                    || !selectedVariations.Add(variationIndex)
                )
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HaveUniqueVariantOptionCombinations(
        CreateProductRequest request,
        IReadOnlyCollection<CreateProductVariantRequest> variants
    )
    {
        if (!HaveValidVariantOptionSelections(request, variants))
        {
            return true;
        }

        var combinations = variants.Select(variant =>
            string.Join(
                "\u001F",
                variant
                    .ProductVariantOptions.Select(option => option.VariationOptionClientId)
                    .Order(StringComparer.Ordinal)
            )
        );

        return combinations.Distinct(StringComparer.Ordinal).Count() == variants.Count;
    }

    private static bool TryBuildVariationIndexByOptionClientId(
        IReadOnlyCollection<CreateVariationRequest> variations,
        out Dictionary<string, int> variationByOption
    )
    {
        variationByOption = new Dictionary<string, int>(StringComparer.Ordinal);
        var variationIndex = 0;

        foreach (var variation in variations)
        {
            if (variation.VariationOptions is null)
            {
                return false;
            }

            foreach (var option in variation.VariationOptions)
            {
                if (
                    string.IsNullOrWhiteSpace(option.ClientId)
                    || !variationByOption.TryAdd(option.ClientId, variationIndex)
                )
                {
                    return false;
                }
            }

            variationIndex++;
        }

        return true;
    }
}
