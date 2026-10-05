using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using SoraSora.Modules.Orders.Contracts.Requests;
using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Contracts.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator(
        IValidator<CreateOrderItemRequest> createOrderItemRequestValidator
    )
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(Order.MaxCustomerNameLength);
        RuleFor(x => x.CustomerEmail).MaximumLength(Order.MaxCustomerEmailLength);
        RuleFor(x => x.CustomerPhoneNumber).NotEmpty().MaximumLength(Order.MaxCustomerPhoneNumberLength);
        RuleFor(x => x.CustomerNote).MaximumLength(Order.MaxCustomerNoteLength);
        RuleFor(x => x.StoreNote).MaximumLength(Order.MaxStoreNoteLength);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(Order.MaxShippingAddressLength);
        RuleForEach(x => x.OrderItems).NotNull().SetValidator(createOrderItemRequestValidator);
        RuleFor(x => x.OrderItems).NotEmpty();
    }
}
