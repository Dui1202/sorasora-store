using SoraSora.Api.Contracts.Commands.OrderWorkflow;
using SoraSora.Api.Contracts.Requests;

namespace SoraSora.Api.Mappers;

public static class CreateOrderMapper
{
    public static PlaceOrderWorkflowCommand ToPlaceOrderWorkflowCommand(
        PlaceOrderWorkflowRequest request
    )
    {
        return new PlaceOrderWorkflowCommand
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            ShippingAddress = request.ShippingAddress,
            CustomerNote = request.CustomerNote,
            OrderItems = request.OrderItems.Select(ToPlaceOrderItemCommand).ToList(),
        };
    }

    public static CreateOrderWorkflowCommand ToCreateOrderWorkflowCommand(
        CreateOrderRequest request
    )
    {
        return new CreateOrderWorkflowCommand
        {
            WarehouseId = request.WarehouseId,
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
