using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using SoraSora.Modules.Orders.Contracts.Requests;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator(
        IValidator<CreateOrderItemRequest> createOrderItemRequestValidator
    )
    {
        RuleFor(x => x.CustomerName).MaximumLength(Order.MaxCustomerNameLength);
        RuleFor(x => x.CustomerEmail).MaximumLength(Order.MaxCustomerEmailLength);
        RuleFor(x => x.CustomerPhoneNumber).MaximumLength(Order.MaxCustomerPhoneNumberLength);
        RuleFor(x => x.CustomerNote).MaximumLength(Order.MaxCustomerNoteLength);
        RuleFor(x => x.StoreNote).MaximumLength(Order.MaxStoreNoteLength);
        RuleFor(x => x.ShippingAddress).MaximumLength(Order.MaxShippingAddressLength);
        RuleForEach(x => x.OrderItems).SetValidator(createOrderItemRequestValidator);
    }
}
