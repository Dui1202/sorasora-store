using SoraSora.Modules.Orders.Contracts.Responses;

namespace SoraSora.Modules.Orders.Mappers;

internal static class OrderDtoMapper
{
    internal static OrderResponse ToOrderResponse(Order order)
    {
        return new OrderResponse(
            Id: order.Id,
            OrderItems: order.OrderItems.Select(ToOrderItemResponse).ToList(),
            TotalPrice: order.TotalPrice,
            CustomerName: order.CustomerName,
            CustomerEmail: order.CustomerEmail,
            CustomerPhoneNumber: order.CustomerPhoneNumber,
            ShippingAddress: order.ShippingAddress,
            Status: order.Status,
            CustomerNote: order.CustomerNote,
            StoreNote: order.StoreNote,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt
        );
    }

    internal static OrderItemResponse ToOrderItemResponse(OrderItem orderItem)
    {
        return new OrderItemResponse(
            Id: orderItem.Id,
            SellingPrice: orderItem.SellingPrice,
            OrderId: orderItem.OrderId,
            ProductVariantId: orderItem.ProductVariantId,
            Quantity: orderItem.Quantity
        );
    }
}
