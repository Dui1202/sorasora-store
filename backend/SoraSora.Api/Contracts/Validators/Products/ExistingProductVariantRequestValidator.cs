using FluentValidation;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Api.Contracts.Requests.Products.Update;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Contracts.Validators.Products;

public sealed class ExistingProductVariantRequestValidator
    : AbstractValidator<RetainedProductVariantRequest>
{
    public ExistingProductVariantRequestValidator(
        IValidator<CreateProductVariantImageRequest> newImageValidator,
        IValidator<CreateProductVariantOptionRequest> variantOptionValidator
    )
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductVariant.MaxNameLength);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Action).IsInEnum();
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Action == InventoryUpdateAction.SetTo);
        RuleFor(x => x.ProductVariantOptions).NotNull();
        RuleForEach(x => x.ProductVariantOptions).SetValidator(variantOptionValidator);

        RuleFor(x => x.RetainedImage)
            .Must(
                (request, retainedImage) =>
                    (request.NewImage is not null) != (retainedImage is not null)
            )
            .WithMessage("Exactly one of NewImage or RetainedImage must be provided.");

        When(
            x => x.RetainedImage is not null,
            () =>
            {
                RuleFor(x => x.RetainedImage!.MediaAssetId).NotEmpty();
                RuleFor(x => x.RetainedImage!.Alt).MaximumLength(MediaAsset.MaxAltLength);
            }
        );

        When(
            x => x.NewImage is not null,
            () => RuleFor(x => x.NewImage!).SetValidator(newImageValidator)
        );
    }
}
