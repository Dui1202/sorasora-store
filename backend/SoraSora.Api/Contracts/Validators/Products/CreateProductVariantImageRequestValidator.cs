using FluentValidation;
using SoraSora.Api.Contracts.Requests.Products.Create;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Api.Contracts.Validators.Products;

public sealed class CreateProductVariantImageRequestValidator
    : AbstractValidator<CreateProductVariantImageRequest>
{
    public CreateProductVariantImageRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Alt).MaximumLength(MediaAsset.MaxAltLength);
    }
}
