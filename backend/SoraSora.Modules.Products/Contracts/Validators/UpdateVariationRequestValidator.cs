using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Update;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class UpdateVariationRequestValidator : AbstractValidator<UpdateVariationRequest>
{
    public UpdateVariationRequestValidator(
        IValidator<UpdateVariationOptionRequest> variationOptionValidator
    )
    {
        When(x => x.Id is not null, () => RuleFor(x => x.Id!).NotEmpty());
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VariationOptions)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .NotEmpty()
            .Must(HaveUniqueClientIds)
            .WithMessage("Variation option client ids must be unique.")
            .Must(HaveUniqueIds)
            .WithMessage("Variation option ids must be unique.")
            .Must(HaveUniqueValues)
            .WithMessage("Variation option values must be unique.")
            .Must(HaveUniquePositions)
            .WithMessage("Variation option positions must be unique.");
        RuleForEach(x => x.VariationOptions).SetValidator(variationOptionValidator);
    }

    private static bool HaveUniqueClientIds(
        IReadOnlyCollection<UpdateVariationOptionRequest> options
    ) =>
        options.Select(x => x.ClientId).Distinct(StringComparer.Ordinal).Count() == options.Count;

    private static bool HaveUniqueIds(IReadOnlyCollection<UpdateVariationOptionRequest> options)
    {
        var ids = options.Where(x => x.Id is not null).Select(x => x.Id!);
        return ids.Distinct(StringComparer.Ordinal).Count() == ids.Count();
    }

    private static bool HaveUniqueValues(
        IReadOnlyCollection<UpdateVariationOptionRequest> options
    )
    {
        if (options.Any(x => string.IsNullOrWhiteSpace(x.Value)))
        {
            return true;
        }

        return options.Select(x => x.Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            == options.Count;
    }

    private static bool HaveUniquePositions(
        IReadOnlyCollection<UpdateVariationOptionRequest> options
    ) => options.Select(x => x.Position).Distinct().Count() == options.Count;
}
