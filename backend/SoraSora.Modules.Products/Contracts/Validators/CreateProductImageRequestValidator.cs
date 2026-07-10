using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

internal sealed class CreateProductImageRequestValidator
    : AbstractValidator<CreateProductImageRequest>
{
    public CreateProductImageRequestValidator()
    {
        RuleFor(x => x.File).NotEmpty();
        RuleFor(x => x.Alt).MaximumLength(200);
        RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
    }
}
