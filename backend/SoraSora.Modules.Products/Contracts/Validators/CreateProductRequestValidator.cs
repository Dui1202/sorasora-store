using FluentValidation;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator(
        IValidator<CreateProductImageRequest> createProductImageRequestValidator,
        IValidator<CreateProductVariantRequest> createProductVariantRequestValidator
    )
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Product.MaxDescriptionLength);
        RuleFor(x => x.SupplierCountry).NotNull().IsInEnum();
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
        RuleFor(x => x.HasVariation).NotNull();

        When(
            request => !request.HasVariation,
            () =>
            {
                RuleFor(request => request.BaseCostPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.BaseSellingPrice).NotNull().GreaterThanOrEqualTo(0);
                RuleFor(request => request.ProductVariants).Empty();
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
                    .WithMessage("Product variant positions must be unique.");
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
}
