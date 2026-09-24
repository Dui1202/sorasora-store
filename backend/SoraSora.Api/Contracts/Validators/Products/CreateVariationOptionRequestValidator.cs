using FluentValidation;
using SoraSora.Api.Contracts.Requests.Products.Create;

namespace SoraSora.Api.Contracts.Validators.Products;

public sealed class CreateVariationOptionRequestValidator
    : AbstractValidator<CreateVariationOptionRequest>
{
    public CreateVariationOptionRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
