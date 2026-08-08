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
        IValidator<RetainedProductVariantRequest> existingVariantValidator
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
            .Must((request, _) => HaveUniquePaths(request))
            .WithMessage("Retained product images paths must be unique.");
        RuleFor(x => x.NewProductVariants).NotNull();
        RuleFor(x => x.RetainedProductVariants)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must((request, _) => HaveUniqueVariantIds(request))
            .WithMessage("Retained product variant ids must be unique.");
        RuleForEach(x => x.RetainedImages).SetValidator(existingImageValidator);
        RuleForEach(x => x.NewImages).SetValidator(newImageValidator);
        RuleForEach(x => x.RetainedProductVariants).SetValidator(existingVariantValidator);
        RuleForEach(x => x.NewProductVariants).SetValidator(newVariantValidator);

        When(
            request =>
                !request.HasVariation
                && request.RetainedProductVariants is not null
                && request.NewProductVariants is not null,
            () =>
            {
                RuleFor(request => request.RetainedProductVariants)
                    .Cascade(CascadeMode.Stop)
                    .Must(
                        (request, retainedVariants) =>
                            retainedVariants.Count + request.NewProductVariants.Count == 1
                    )
                    .WithMessage(
                        "A product must contain exactly one variant when variations are disabled."
                    );
            }
        );
        When(
            request =>
                request.HasVariation
                && request.RetainedProductVariants is not null
                && request.NewProductVariants is not null,
            () =>
            {
                RuleFor(request => request.RetainedProductVariants)
                    .Cascade(CascadeMode.Stop)
                    .Must(
                        (request, existingVariants) =>
                            existingVariants.Count + request.NewProductVariants.Count > 0
                    )
                    .WithMessage("A product with variations must contain at least one variant.")
                    .Must(
                        (request, existingVariants) =>
                            existingVariants.Count + request.NewProductVariants.Count
                            <= ProductUploadLimits.MaxProductVariants
                    )
                    .WithMessage(
                        $"A product can contain at most {ProductUploadLimits.MaxProductVariants} variants."
                    )
                    .Must((request, _) => HaveUniqueVariantPositions(request))
                    .WithMessage("Product variant positions must be unique.");
            }
        );
    }

    private static bool HaveUniqueImagePositions(UpdateProductRequest request)
    {
        var positions = request
            .RetainedImages.Select(variant => variant.Position)
            .Concat(request.NewImages.Select(variant => variant.Position));

        var imageCount = request.RetainedImages.Count + request.NewImages.Count;

        return positions.Distinct().Count() == imageCount;
    }

    private static bool HaveUniqueVariantPositions(UpdateProductRequest request)
    {
        var positions = request
            .RetainedProductVariants.Select(variant => variant.Position)
            .Concat(request.NewProductVariants.Select(variant => variant.Position));

        var variantCount = request.RetainedProductVariants.Count + request.NewProductVariants.Count;

        return positions.Distinct().Count() == variantCount;
    }

    private static bool HaveUniqueVariantIds(UpdateProductRequest request)
    {
        var ids = request.RetainedProductVariants.Select(variant => variant.Id);

        var retainedVariantCount = request.RetainedProductVariants.Count;

        return ids.Distinct().Count() == retainedVariantCount;
    }

    private static bool HaveUniquePaths(UpdateProductRequest request)
    {
        var paths = request.RetainedImages.Select(variant => variant.Path);

        var retainedImageCount = request.RetainedImages.Count;

        return paths.Distinct().Count() == retainedImageCount;
    }
}
