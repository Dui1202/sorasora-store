using FluentValidation;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Contracts.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name).Length(1, 100).NotEmpty();
            RuleFor(x => x.Description).Length(0, 1000);
        }
    }
}
