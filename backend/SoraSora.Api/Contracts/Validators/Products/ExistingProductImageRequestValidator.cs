using FluentValidation;
using SoraSora.Modules.Media.Domain;
using SoraSora.Api.Contracts.Requests.Products.Update;

namespace SoraSora.Api.Contracts.Validators.Products;

public sealed class ExistingProductImageRequestValidator
    : AbstractValidator<RetainedProductImageRequest>
{
    public ExistingProductImageRequestValidator()
    {
        RuleFor(x => x.MediaAssetId).NotEmpty();
        RuleFor(x => x.Alt).MaximumLength(MediaAsset.MaxAltLength);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
