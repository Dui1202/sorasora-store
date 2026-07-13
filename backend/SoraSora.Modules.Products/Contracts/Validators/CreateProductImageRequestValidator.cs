using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators
{
    public sealed class CreateProductImageRequestValidator
        : AbstractValidator<CreateProductImageRequest>
    {
        private const long MaxImageSize = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp",
        ];

        public CreateProductImageRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .Must(file => file.Length > 0)
                .WithMessage("Image file cannot be empty.")
                .Must(file => file.Length <= MaxImageSize)
                .WithMessage("Image file cannot exceed 5 MB.")
                .Must(file => AllowedImageContentTypes.Contains(file.ContentType))
                .WithMessage("Only JPEG, PNG, and WebP images are allowed.");
            RuleFor(x => x.Alt).MaximumLength(200);
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
        }
    }
}
