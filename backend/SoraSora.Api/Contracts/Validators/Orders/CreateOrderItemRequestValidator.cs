using FluentValidation;
using SoraSora.Api.Contracts.Requests;

namespace SoraSora.Api.Contracts.Validators.Orders;

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.SaleType).IsInEnum();
    }
}
