using Microsoft.EntityFrameworkCore;

using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<OrderResponse> CancelOrderAsync(
        string orderId,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed order.");
        }

        order.SetStatus(OrderStatus.Cancelled);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }

    public async Task<OrderResponse> ConfirmOrderAsync(
        string orderId,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Cannot confirm a order that is not in pending.");
        }
        order.SetStatus(OrderStatus.Confirmed);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }

    public async Task<OrderResponse> CompleteOrderAsync(
        string orderId,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        if (order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Cannot complete an order that is not in progress");
        }
        order.SetStatus(OrderStatus.Completed);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }
}
