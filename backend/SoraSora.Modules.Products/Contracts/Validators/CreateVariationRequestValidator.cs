using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateVariationRequestValidator : AbstractValidator<CreateVariationRequest>
{
    public CreateVariationRequestValidator(
        IValidator<CreateVariationOptionRequest> variationOptionValidator
    )
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VariationOptions)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(HaveUniqueClientIds)
            .WithMessage("Variation option client ids must be unique.")
            .Must(HaveUniqueValues)
            .WithMessage("Variation option values must be unique.")
            .Must(HaveUniquePositions)
            .WithMessage("Variation option positions must be unique.");
        RuleForEach(x => x.VariationOptions).SetValidator(variationOptionValidator);
    }

    private static bool HaveUniqueClientIds(
        IReadOnlyCollection<CreateVariationOptionRequest> options
    )
    {
        return options.Select(x => x.ClientId).Distinct(StringComparer.Ordinal).Count()
            == options.Count;
    }

    private static bool HaveUniqueValues(
        IReadOnlyCollection<CreateVariationOptionRequest> options
    )
    {
        if (options.Any(x => string.IsNullOrWhiteSpace(x.Value)))
        {
            return true;
        }

        return options
                .Select(x => x.Value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
            == options.Count;
    }

    private static bool HaveUniquePositions(
        IReadOnlyCollection<CreateVariationOptionRequest> options
    )
    {
        return options.Select(x => x.Position).Distinct().Count() == options.Count;
    }
}
