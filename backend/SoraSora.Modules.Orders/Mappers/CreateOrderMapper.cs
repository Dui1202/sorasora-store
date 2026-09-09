using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Requests;

namespace SoraSora.Modules.Orders.Mappers;

internal static class CreateOrderDtoMapper
{
    internal static CreateOrderCommand ToCreateOrderCommand(CreateOrderRequest request)
    {
        return new CreateOrderCommand
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            ShippingAddress = request.ShippingAddress,
            CustomerNote = request.CustomerNote,
            StoreNote = request.StoreNote,
            OrderItems = request.OrderItems.Select(ToCreateOrderItemCommand).ToList(),
        };
    }

    internal static CreateOrderItemCommand ToCreateOrderItemCommand(CreateOrderItemRequest request)
    {
        return new CreateOrderItemCommand
        {
            InventoryItemId = request.InventoryItemId,
            SellingPrice = request.SellingPrice,
            Quantity = request.Quantity,
        };
    }
}
