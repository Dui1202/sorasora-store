using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Update;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class ExistingProductImageRequestValidator
    : AbstractValidator<RetainedProductImageRequest>
{
    public ExistingProductImageRequestValidator()
    {
        RuleFor(x => x.Path).NotEmpty();
        RuleFor(x => x.Alt).MaximumLength(200);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
