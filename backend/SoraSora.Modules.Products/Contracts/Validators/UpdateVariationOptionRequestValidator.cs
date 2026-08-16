using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Update;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class UpdateVariationOptionRequestValidator
    : AbstractValidator<UpdateVariationOptionRequest>
{
    public UpdateVariationOptionRequestValidator()
    {
        When(x => x.Id is not null, () => RuleFor(x => x.Id!).NotEmpty());
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
