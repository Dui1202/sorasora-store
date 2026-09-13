using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using SoraSora.Modules.Orders.Contracts.Requests;

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}
