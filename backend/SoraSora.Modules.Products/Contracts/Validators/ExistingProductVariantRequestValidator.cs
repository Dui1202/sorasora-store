using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class ExistingProductVariantRequestValidator
    : AbstractValidator<RetainedProductVariantRequest>
{
    public ExistingProductVariantRequestValidator(
        IValidator<CreateProductVariantImageRequest> newImageValidator
    )
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductVariant.MaxNameLength);
        RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);

        When(
            x => x.NewImage is not null,
            () => RuleFor(x => x.NewImage!).SetValidator(newImageValidator)
        );
    }
}
