using FluentValidation;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators;

public sealed class CreateProductImageRequestValidator
    : AbstractValidator<CreateProductImageRequest>
{
    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("Image file cannot be empty.")
            .Must(file => file.Length <= ProductUploadLimits.MaxImageBytes)
            .WithMessage("Image file cannot exceed 5 MB.");
        RuleFor(x => x.Alt).MaximumLength(200);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
