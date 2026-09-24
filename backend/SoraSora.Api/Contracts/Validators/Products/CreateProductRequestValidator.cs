using FluentValidation;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Contracts.Validators.Products;

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
        RuleFor(x => x.SupplierCountry).IsInEnum();
        RuleFor(x => x.SellMode).IsInEnum();
        RuleFor(x => x.BaseQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProductImages)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(images => images.Count <= ProductUploadLimits.MaxProductImages)
            .WithMessage(
                $"A product can contain at most {ProductUploadLimits.MaxProductImages} images."
            )
            .Must(HaveUniquePositions)
            .WithMessage("Product image positions must be unique.")
            .Must(HaveUniqueProductImageClientIds)
            .WithMessage("Product image client ids must be unique.");
        RuleFor(x => x.ProductVariants).NotNull();
        RuleFor(x => x.Variations).NotNull();
        RuleForEach(x => x.ProductImages).SetValidator(createProductImageRequestValidator);
        RuleForEach(x => x.ProductVariants).SetValidator(createProductVariantRequestValidator);
        RuleForEach(x => x.Variations).SetValidator(createVariationRequestValidator);

        When(
            request => request.Variations is { Count: 0 },
            () =>
            {
                RuleFor(request => request.BaseCostPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.BaseSellingPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.ProductVariants).Empty();
                RuleFor(request => request.Variations).Empty();
            }
        );

        When(
            request => request.Variations is { Count: > 0 },
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
                    .Must(HaveUniqueVariantImageClientIds)
                    .WithMessage("Product variant image client ids must be unique.")
                    .Must(
                        (request, variants) => HaveValidVariantOptionSelections(request, variants)
                    )
                    .WithMessage(
                        "Each product variant must select exactly one valid option for each variation."
                    )
                    .Must(
                        (request, variants) =>
                            HaveUniqueVariantOptionCombinations(request, variants)
                    )
                    .WithMessage("Product variant option combinations must be unique.");

                RuleFor(request => request.Variations)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .Must(HaveUniqueVariationClientIds)
                    .WithMessage("Variation client ids must be unique.")
                    .Must(HaveUniqueVariationPositions)
                    .WithMessage("Variation positions must be unique.")
                    .Must(HaveUniqueVariationNames)
                    .WithMessage("Variation names must be unique.")
                    .Must(HaveGloballyUniqueVariationOptionClientIds)
                    .WithMessage("Variation option client ids must be unique across the product.");
            }
        );
    }

    private static bool HaveUniqueProductImageClientIds(
        IReadOnlyCollection<CreateProductImageRequest> images
    )
    {
        return images.Select(image => image.ClientId).Distinct(StringComparer.Ordinal).Count()
            == images.Count;
    }

    private static bool HaveUniqueVariantImageClientIds(
        IReadOnlyCollection<CreateProductVariantRequest> variants
    )
    {
        // Missing images are reported by the variant validator.
        var imageClientIds = variants
            .Where(variant => variant.ProductVariantImage is not null)
            .Select(variant => variant.ProductVariantImage.ClientId)
            .ToList();

        return imageClientIds.Distinct(StringComparer.Ordinal).Count() == imageClientIds.Count;
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

    private static bool HaveUniqueVariationClientIds(
        IReadOnlyCollection<CreateVariationRequest> variations
    )
    {
        if (variations.Any(x => string.IsNullOrWhiteSpace(x.ClientId)))
        {
            return true;
        }

        return variations
                .Select(x => x.ClientId)
                .Distinct(StringComparer.Ordinal)
                .Count() == variations.Count;
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
                .Count() == variations.Count;
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
            || !TryBuildVariationClientIdByOptionClientId(
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

            var selectedVariationClientIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var selectedOption in variant.ProductVariantOptions)
            {
                if (
                    !variationByOption.TryGetValue(
                        selectedOption.VariationOptionClientId,
                        out var expectedVariationClientId
                    )
                    || !string.Equals(
                        selectedOption.VariationClientId,
                        expectedVariationClientId,
                        StringComparison.Ordinal
                    )
                    || !selectedVariationClientIds.Add(selectedOption.VariationClientId)
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
                variant.ProductVariantOptions
                    .Select(option =>
                        $"{option.VariationClientId}\u001E{option.VariationOptionClientId}"
                    )
                    .Order(StringComparer.Ordinal)
            )
        );

        return combinations.Distinct(StringComparer.Ordinal).Count() == variants.Count;
    }

    private static bool TryBuildVariationClientIdByOptionClientId(
        IReadOnlyCollection<CreateVariationRequest> variations,
        out Dictionary<string, string> variationByOption
    )
    {
        variationByOption = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var variation in variations)
        {
            if (
                string.IsNullOrWhiteSpace(variation.ClientId)
                || variation.VariationOptions is null
            )
            {
                return false;
            }

            foreach (var option in variation.VariationOptions)
            {
                if (
                    string.IsNullOrWhiteSpace(option.ClientId)
                    || !variationByOption.TryAdd(option.ClientId, variation.ClientId)
                )
                {
                    return false;
                }
            }
        }

        return true;
    }
}
