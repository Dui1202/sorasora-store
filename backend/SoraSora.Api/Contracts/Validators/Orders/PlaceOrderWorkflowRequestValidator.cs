using FluentValidation;
using SoraSora.Api.Contracts.Requests;
using SoraSora.Modules.Orders.Contracts.Requests;
using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Api.Contracts.Validators.Orders;

public sealed class PlaceOrderWorkflowRequestValidator : AbstractValidator<PlaceOrderWorkflowRequest>
{
    public PlaceOrderWorkflowRequestValidator(IValidator<CreateOrderItemRequest> orderItemValidator)
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(Order.MaxCustomerNameLength);
        RuleFor(x => x.CustomerEmail).MaximumLength(Order.MaxCustomerEmailLength);
        RuleFor(x => x.CustomerPhoneNumber).NotEmpty().MaximumLength(Order.MaxCustomerPhoneNumberLength);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(Order.MaxShippingAddressLength);
        RuleFor(x => x.CustomerNote).MaximumLength(Order.MaxCustomerNoteLength);
        RuleFor(x => x.OrderItems).NotEmpty();
        RuleForEach(x => x.OrderItems).NotNull().SetValidator(orderItemValidator);
    }
}
