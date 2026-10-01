using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Domain;
using SoraSora.Modules.Orders.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Orders.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<OrderResponse> CancelOrderAsync(
        string orderId,
        Guid version,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        VersionValidation(order, version);

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed order.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot cancel an already cancelled order.");
        }

        order.SetStatus(OrderStatus.Cancelled);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }

    public async Task<OrderResponse> ConfirmOrderAsync(
        string orderId,
        Guid version,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        VersionValidation(order, version);

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
        Guid version,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken)
            ?? throw new NotFoundException("Order is not found.");

        VersionValidation(order, version);

        if (order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException("Cannot complete an order that is not in progress");
        }
        order.SetStatus(OrderStatus.Completed);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return OrderDtoMapper.ToOrderResponse(order);
    }

    private void VersionValidation(Order order, Guid vesion)
    {
        if (order.Version != vesion)
        {
            throw new ConflictException(
                "Order was changed by other operation. Reload and try again!"
            );
        }

        order.AdvanceVersion();
    }
}
