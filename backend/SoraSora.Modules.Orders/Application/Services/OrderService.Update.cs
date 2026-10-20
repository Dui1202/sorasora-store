using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Exceptions;
using SoraSora.Modules.Orders.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Orders.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<OrderResponse> UpdateOrderMetadataAsync(
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

        VersionValidation(order, command.Version);

        try
        {
            order.UpdateDetails(
                customerName: command.CustomerName,
                customerPhoneNumber: command.CustomerPhoneNumber,
                shippingAddress: command.ShippingAddress,
                customerEmail: command.CustomerEmail,
                customerNote: command.CustomerNote,
                storeNote: command.StoreNote
            );

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new OrderConcurrencyException(order.Id, exception);
        }

        return OrderDtoMapper.ToOrderResponse(order);
    }
}
