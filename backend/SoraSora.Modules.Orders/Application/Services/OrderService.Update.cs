using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Domain;
using SoraSora.Modules.Orders.Exceptions;
using SoraSora.Modules.Orders.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Orders.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<OrderResponse> UpdateOrderAsync(
        UpdateOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        order.UpdateDetails(
            customerName: command.CustomerName,
            customerPhoneNumber: command.CustomerPhoneNumber,
            shippingAddress: command.ShippingAddress,
            customerEmail: command.CustomerEmail,
            customerNote: command.CustomerNote,
            storeNote: command.StoreNote
        );

        var normalizedExistingOrderItemIds = order
            .OrderItems.Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var retainedOrderItemIds = command
            .OrderItems.Where(x => x.Id is not null)
            .Select(x => x.Id)
            .ToHashSet(StringComparer.Ordinal);

        var unretainedOrderItemIds = normalizedExistingOrderItemIds
            .Except(retainedOrderItemIds)
            .ToHashSet(StringComparer.Ordinal);

        UpdateRetainedOrderItems(order, command, retainedOrderItemIds);
        CreateNewOrderItems(order, command);
        RemoveUnretainedOrderItems(order, unretainedOrderItemIds);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }

    private void UpdateRetainedOrderItems(
        Order order,
        UpdateOrderCommand command,
        HashSet<string?> retainedOrderItemIds
    )
    {
        var retainedOrderItemsById = order
            .OrderItems.Where(x => retainedOrderItemIds.Contains(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var retainedOrderItemCommands = command
            .OrderItems.Where(x => retainedOrderItemIds.Contains(x.Id))
            .ToList();

        foreach (var itemCommand in retainedOrderItemCommands)
        {
            if (!retainedOrderItemsById.TryGetValue(itemCommand.Id!, out var orderItem))
            {
                throw new NotFoundException("Order item is not found.");
            }

            orderItem.UpdateDetails(
                sellingPrice: itemCommand.SellingPrice,
                quantity: itemCommand.Quantity
            );
        }
    }

    private void CreateNewOrderItems(Order order, UpdateOrderCommand command)
    {
        var newOrderItemsCommand = command.OrderItems.Where(x => x.Id is null).ToList();

        foreach (var item in newOrderItemsCommand)
        {
            if (string.IsNullOrWhiteSpace(item.ProductVariantId))
            {
                throw new OrderDomainException(
                    "Product variant ID is required for a new order item.",
                    nameof(item.ProductVariantId)
                );
            }
            var newOrderItem = OrderItem.Create(
                productName: item.ProductName!,
                productVariantId: item.ProductVariantId,
                sellingPrice: item.SellingPrice,
                quantity: item.Quantity,
                sku: item.Sku!
            );

            order.AddOrderItem(newOrderItem);
        }
    }

    private void RemoveUnretainedOrderItems(Order order, HashSet<string?> unretainedOrderItemIds)
    {
        var orderItemToDelete = order
            .OrderItems.Where(x => unretainedOrderItemIds.Contains(x.Id))
            .ToList();

        foreach (var item in orderItemToDelete)
        {
            order.RemoveOrderItem(item);
        }
    }
}
