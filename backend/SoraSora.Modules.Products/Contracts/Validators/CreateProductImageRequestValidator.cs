using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateProductImageRequestValidator
    : AbstractValidator<CreateProductImageRequest>
{
    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Alt).MaximumLength(MediaAsset.MaxAltLength);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
