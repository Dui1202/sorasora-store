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

        try
        {
            order.Cancel();

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is DbUpdateConcurrencyException)
            {
                throw new OrderConcurrencyException(order.Id, exception);
            }

            throw;
        }

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
        try
        {
            order.Confirm();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is DbUpdateConcurrencyException)
            {
                throw new OrderConcurrencyException(order.Id, exception);
            }

            throw;
        }

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

        try
        {
            order.Complete();

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is DbUpdateConcurrencyException)
            {
                throw new OrderConcurrencyException(order.Id, exception);
            }
            throw;
        }

        return OrderDtoMapper.ToOrderResponse(order);
    }

    private void VersionValidation(Order order, Guid vesion)
    {
        if (order.Version != vesion)
        {
            throw new OrderConcurrencyException(order.Id);
        }

        order.AdvanceVersion();
    }
}
