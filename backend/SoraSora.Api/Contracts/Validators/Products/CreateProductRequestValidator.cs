using FluentValidation;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Modules.Products.Contracts;
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
        RuleFor(x => x.ProductImages)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(images => images.All(image => image is not null))
            .WithMessage("Product images must not contain null entries.")
            .Must(images => images.Count <= ProductUploadLimits.MaxProductImages)
            .WithMessage(
                $"A product can contain at most {ProductUploadLimits.MaxProductImages} images."
            )
            .Must(HaveUniquePositions)
            .WithMessage("Product image positions must be unique.")
            .Must(HaveUniqueProductImageClientIds)
            .WithMessage("Product image client ids must be unique.");
        RuleFor(x => x.ProductVariants)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(HaveCompleteVariants)
            .WithMessage("Product variants and their option selections must not be null or contain null entries.");
        RuleFor(x => x.Variations)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(HaveCompleteVariations)
            .WithMessage("Variations and their options must not be null or contain null entries.");
        RuleForEach(x => x.ProductImages).SetValidator(createProductImageRequestValidator);
        When(
            request => request.ProductVariants is not null && HaveCompleteVariants(request.ProductVariants),
            () => RuleForEach(x => x.ProductVariants).SetValidator(createProductVariantRequestValidator)
        );
        When(
            request => request.Variations is not null && HaveCompleteVariations(request.Variations),
            () => RuleForEach(x => x.Variations).SetValidator(createVariationRequestValidator)
        );

        When(
            request => request.ProductVariants is not null
                && HaveCompleteVariants(request.ProductVariants)
                && request.Variations is not null
                && HaveCompleteVariations(request.Variations),
            () =>
            {
                RuleFor(request => request.ProductVariants)
                    .Cascade(CascadeMode.Stop)
                    .Must((request, productVariants) =>
                        request.Variations.Count == 0
                            ? productVariants.Count == 1
                            : productVariants.Count is > 0 and <= ProductUploadLimits.MaxProductVariants
                    )
                    .WithMessage(
                        $"A product without variations must contain exactly one variant; a product with variations must contain between one and {ProductUploadLimits.MaxProductVariants} variants."
                    )
                    .Must(HaveUniqueVariantPositions)
                    .WithMessage("Product variant positions must be unique.")
                    .Must(HaveUniqueVariantImageClientIds)
                    .WithMessage("Product variant image client ids must be unique.")
                    .Must(
                        (request, variants) => HaveValidVariantOptionSelections(request, variants)
                    )
                    .WithMessage(
                        "Each product variant must select exactly one valid option for each variation, or no options when the product has no variations."
                    )
                    .Must(
                        (request, variants) =>
                            HaveUniqueVariantOptionCombinations(request, variants)
                    )
                    .WithMessage("Product variant option combinations must be unique.");

                RuleFor(request => request.Variations)
                    .Cascade(CascadeMode.Stop)
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

    private static bool HaveCompleteVariants(IReadOnlyCollection<CreateProductVariantRequest> variants)
    {
        return variants.All(variant => variant is not null
            && variant.ProductVariantOptions is not null
            && variant.ProductVariantOptions.All(option => option is not null));
    }

    private static bool HaveCompleteVariations(IReadOnlyCollection<CreateVariationRequest> variations)
    {
        return variations.All(variation => variation is not null
            && variation.VariationOptions is not null
            && variation.VariationOptions.All(option => option is not null));
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
