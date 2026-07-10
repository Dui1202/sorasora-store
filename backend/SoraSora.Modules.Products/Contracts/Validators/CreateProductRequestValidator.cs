using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator(
            IValidator<CreateProductImageRequest> createProductImageRequestValidator
        )
        {
            RuleFor(x => x.Name).Length(1, 200).NotEmpty();
            RuleFor(x => x.Description).MaximumLength(3000);
            RuleFor(x => x.SupplierCountry).NotNull().IsInEnum();
            RuleFor(x => x.IsPublished).NotNull();
            RuleFor(x => x.BaseCostPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BaseSellingPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ProductImages).NotEmpty().Must(HaveUniquePositions);
            RuleForEach(x => x.ProductImages).SetValidator(createProductImageRequestValidator);
            RuleFor(x => x.HasVariation).NotNull();
        }

        private static bool HaveUniquePositions(
            IReadOnlyCollection<CreateProductImageRequest> images
        )
        {
            return images.Select(x => x.Position).Distinct().Count() == images.Count;
        }
    }
}
