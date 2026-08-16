using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator(
        IValidator<CreateProductImageRequest> newImageValidator,
        IValidator<RetainedProductImageRequest> existingImageValidator,
        IValidator<CreateProductVariantRequest> newVariantValidator,
        IValidator<RetainedProductVariantRequest> existingVariantValidator,
        IValidator<UpdateVariationRequest> variationValidator
    )
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength);
        RuleFor(x => x.SupplierCountry).IsInEnum();
        RuleFor(x => x.SellMode).IsInEnum();
        RuleFor(x => x.NewImages).NotNull();
        RuleFor(x => x.RetainedImages)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(
                (request, retainedImages) =>
                    request.NewImages is not null
                    && retainedImages.Count + request.NewImages.Count > 0
            )
            .WithMessage("A product must contain at least one image.")
            .Must(
                (request, retainedImages) =>
                    retainedImages.Count + request.NewImages.Count
                    <= ProductUploadLimits.MaxProductImages
            )
            .WithMessage(
                $"A product can contain at most {ProductUploadLimits.MaxProductImages} images."
            )
            .Must((request, _) => HaveUniqueImagePositions(request))
            .WithMessage("Product image positions must be unique.")
            .Must((request, _) => HaveUniqueMediaAssetIds(request))
            .WithMessage("Retained product image media asset ids must be unique.");

        RuleFor(x => x.NewProductVariants).NotNull();
        RuleFor(x => x.RetainedProductVariants)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must((request, _) => HaveUniqueVariantIds(request))
            .WithMessage("Retained product variant ids must be unique.");
        RuleFor(x => x.Variations)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(HaveUniqueVariationClientIds)
            .WithMessage("Variation client ids must be unique.")
            .Must(HaveUniqueVariationIds)
            .WithMessage("Variation ids must be unique.")
            .Must(HaveUniqueVariationPositions)
            .WithMessage("Variation positions must be unique.")
            .Must(HaveUniqueVariationNames)
            .WithMessage("Variation names must be unique.")
            .Must(HaveGloballyUniqueVariationOptionClientIds)
            .WithMessage("Variation option client ids must be unique across the product.")
            .Must(HaveGloballyUniqueVariationOptionIds)
            .WithMessage("Variation option ids must be unique across the product.");

        RuleForEach(x => x.RetainedImages).SetValidator(existingImageValidator);
        RuleForEach(x => x.NewImages).SetValidator(newImageValidator);
        RuleForEach(x => x.RetainedProductVariants).SetValidator(existingVariantValidator);
        RuleForEach(x => x.NewProductVariants).SetValidator(newVariantValidator);
        RuleForEach(x => x.Variations).SetValidator(variationValidator);

        When(
            HasCompleteCollections,
            () =>
            {
                RuleFor(x => x.RetainedProductVariants)
                    .Must((request, _) => HaveValidVariantCount(request))
                    .WithMessage(
                        "A product without variations must contain exactly one variant; a product with variations must contain between one and the configured maximum."
                    )
                    .Must((request, _) => HaveUniqueVariantPositions(request))
                    .WithMessage("Product variant positions must be unique.")
                    .Must((request, _) => HaveValidVariantOptionSelections(request))
                    .WithMessage(
                        "Each product variant must select exactly one valid option for each variation."
                    )
                    .Must((request, _) => HaveUniqueVariantOptionCombinations(request))
                    .WithMessage("Product variant option combinations must be unique.");
            }
        );
    }

    private static bool HasCompleteCollections(UpdateProductRequest request) =>
        request.Variations is not null
        && request.RetainedProductVariants is not null
        && request.NewProductVariants is not null
        && request.Variations.All(x => x.VariationOptions is not null)
        && request.RetainedProductVariants.All(x => x.ProductVariantOptions is not null)
        && request.NewProductVariants.All(x => x.ProductVariantOptions is not null);

    private static bool HaveValidVariantCount(UpdateProductRequest request)
    {
        var count = request.RetainedProductVariants.Count + request.NewProductVariants.Count;
        return request.Variations.Count == 0
            ? count == 1
            : count is > 0 and <= ProductUploadLimits.MaxProductVariants;
    }

    private static bool HaveUniqueImagePositions(UpdateProductRequest request)
    {
        var positions = request.RetainedImages.Select(x => x.Position)
            .Concat(request.NewImages.Select(x => x.Position));
        return positions.Distinct().Count()
            == request.RetainedImages.Count + request.NewImages.Count;
    }

    private static bool HaveUniqueVariantPositions(UpdateProductRequest request)
    {
        var positions = request.RetainedProductVariants.Select(x => x.Position)
            .Concat(request.NewProductVariants.Select(x => x.Position));
        return positions.Distinct().Count()
            == request.RetainedProductVariants.Count + request.NewProductVariants.Count;
    }

    private static bool HaveUniqueVariantIds(UpdateProductRequest request)
    {
        var ids = request.RetainedProductVariants.Select(x => x.Id);
        return ids.Distinct(StringComparer.Ordinal).Count()
            == request.RetainedProductVariants.Count;
    }

    private static bool HaveUniqueMediaAssetIds(UpdateProductRequest request)
    {
        var ids = request.RetainedImages.Select(x => x.MediaAssetId);
        return ids.Distinct(StringComparer.Ordinal).Count() == request.RetainedImages.Count;
    }

    private static bool HaveUniqueVariationClientIds(
        IReadOnlyCollection<UpdateVariationRequest> variations
    ) =>
        variations.Select(x => x.ClientId).Distinct(StringComparer.Ordinal).Count()
        == variations.Count;

    private static bool HaveUniqueVariationIds(
        IReadOnlyCollection<UpdateVariationRequest> variations
    )
    {
        var ids = variations.Where(x => x.Id is not null).Select(x => x.Id!).ToList();
        return ids.Distinct(StringComparer.Ordinal).Count() == ids.Count;
    }

    private static bool HaveUniqueVariationPositions(
        IReadOnlyCollection<UpdateVariationRequest> variations
    ) => variations.Select(x => x.Position).Distinct().Count() == variations.Count;

    private static bool HaveUniqueVariationNames(
        IReadOnlyCollection<UpdateVariationRequest> variations
    )
    {
        if (variations.Any(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            return true;
        }

        return variations.Select(x => x.Name.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            == variations.Count;
    }

    private static bool HaveGloballyUniqueVariationOptionClientIds(
        IReadOnlyCollection<UpdateVariationRequest> variations
    )
    {
        if (variations.Any(x => x.VariationOptions is null))
        {
            return true;
        }

        var options = variations.SelectMany(x => x.VariationOptions).ToList();
        return options.Select(x => x.ClientId).Distinct(StringComparer.Ordinal).Count()
            == options.Count;
    }

    private static bool HaveGloballyUniqueVariationOptionIds(
        IReadOnlyCollection<UpdateVariationRequest> variations
    )
    {
        if (variations.Any(x => x.VariationOptions is null))
        {
            return true;
        }

        var ids = variations
            .SelectMany(x => x.VariationOptions)
            .Where(x => x.Id is not null)
            .Select(x => x.Id!)
            .ToList();
        return ids.Distinct(StringComparer.Ordinal).Count() == ids.Count;
    }

    private static bool HaveValidVariantOptionSelections(UpdateProductRequest request)
    {
        var selections = GetVariantOptionSelections(request);
        if (request.Variations.Count == 0)
        {
            return selections.All(x => x.Count == 0);
        }

        if (
            !TryBuildVariationClientIdByOptionClientId(
                request.Variations,
                out var variationByOption
            )
        )
        {
            return false;
        }

        foreach (var variantSelections in selections)
        {
            if (variantSelections.Count != request.Variations.Count)
            {
                return false;
            }

            var selectedVariationClientIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var selection in variantSelections)
            {
                if (
                    !variationByOption.TryGetValue(
                        selection.VariationOptionClientId,
                        out var expectedVariationClientId
                    )
                    || !string.Equals(
                        selection.VariationClientId,
                        expectedVariationClientId,
                        StringComparison.Ordinal
                    )
                    || !selectedVariationClientIds.Add(selection.VariationClientId)
                )
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HaveUniqueVariantOptionCombinations(UpdateProductRequest request)
    {
        if (!HaveValidVariantOptionSelections(request))
        {
            return true;
        }

        var combinations = GetVariantOptionSelections(request)
            .Select(options =>
                string.Join(
                    "\u001F",
                    options
                        .Select(x => $"{x.VariationClientId}\u001E{x.VariationOptionClientId}")
                        .Order(StringComparer.Ordinal)
                )
            )
            .ToList();

        return combinations.Distinct(StringComparer.Ordinal).Count() == combinations.Count;
    }

    private static IReadOnlyList<IReadOnlyCollection<CreateProductVariantOptionRequest>> GetVariantOptionSelections(
        UpdateProductRequest request
    )
    {
        return request.RetainedProductVariants
            .Select(x => x.ProductVariantOptions)
            .Concat(request.NewProductVariants.Select(x => x.ProductVariantOptions))
            .ToList();
    }

    private static bool TryBuildVariationClientIdByOptionClientId(
        IReadOnlyCollection<UpdateVariationRequest> variations,
        out Dictionary<string, string> variationByOption
    )
    {
        variationByOption = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var variation in variations)
        {
            foreach (var option in variation.VariationOptions)
            {
                if (!variationByOption.TryAdd(option.ClientId, variation.ClientId))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
