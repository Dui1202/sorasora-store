using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class ExistingProductImageRequestValidator
    : AbstractValidator<RetainedProductImageRequest>
{
    public ExistingProductImageRequestValidator()
    {
        RuleFor(x => x.Path).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.Alt).MaximumLength(ProductImage.MaxAltLength);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
