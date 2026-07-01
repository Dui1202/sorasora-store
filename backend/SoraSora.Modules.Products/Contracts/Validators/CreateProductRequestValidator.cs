using FluentValidation;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<Product>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name).Length(1, 100).NotEmpty();
            RuleFor(x => x.Description).Length(0, 1000);
        }
    }
}
