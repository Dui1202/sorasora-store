using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Mappers;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<OrderResponse> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        var order = Order.Create(
            customerName: command.CustomerName,
            customerPhoneNumber: command.CustomerPhoneNumber,
            shippingAddress: command.ShippingAddress,
            customerEmail: command.CustomerEmail,
            customerNote: command.CustomerNote,
            storeNote: command.StoreNote
        );

        foreach (var itemCommand in command.OrderItems)
        {
            var orderItem = OrderItem.Create(
                productVariantId: itemCommand.ProductVariantId,
                sellingPrice: itemCommand.SellingPrice,
                quantity: itemCommand.Quantity
            );

            order.AddOrderItem(orderItem);
        }

        _dbContext.Orders().Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }
}
