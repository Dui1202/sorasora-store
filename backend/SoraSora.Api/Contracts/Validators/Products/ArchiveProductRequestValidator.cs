using FluentValidation;

public sealed class ArchiveProductRequestValidator : AbstractValidator<ArchiveProductRequest>
{
    public ArchiveProductRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Version).NotEmpty();
    }
}
