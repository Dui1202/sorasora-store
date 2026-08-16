using FluentValidation;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Products.Contracts.Requests.Update;

namespace SoraSora.Modules.Products.Contracts.Validators;

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
