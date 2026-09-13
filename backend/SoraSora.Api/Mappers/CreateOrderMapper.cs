using SoraSora.Modules.Orders.Contracts.Requests;

namespace SoraSora.Api.Mappers;

public static class PlaceOrderWorkflowMapper
{
    public static PlaceOrderWorkflowCommand ToPlaceOrderWorkflowCommand(CreateOrderRequest request)
    {
        return new PlaceOrderWorkflowCommand
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            ShippingAddress = request.ShippingAddress,
            CustomerNote = request.CustomerNote,
            StoreNote = request.StoreNote,
            OrderItems = request.OrderItems.Select(ToPlaceOrderItemCommand).ToList(),
        };
    }

    public static PlaceOrderItemCommand ToPlaceOrderItemCommand(CreateOrderItemRequest request)
    {
        return new PlaceOrderItemCommand
        {
            ProductVariantId = request.ProductVariantId,
            Quantity = request.Quantity,
        };
    }
}
