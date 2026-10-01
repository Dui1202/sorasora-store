using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Infrastructure.Persistence.Extension;
using SoraSora.Modules.Orders.Mappers;
using SoraSora.Shared.Exceptions;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService : IOrderService
{
    public async Task<IReadOnlyCollection<OrderResponse>> GetAllOrdersAsync(
        CancellationToken cancellationToken
    )
    {
        var orders = await _dbContext
            .Orders()
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .OrderBy(x => x.CreatedAt)
            .Select(x => OrderDtoMapper.ToOrderResponse(x))
            .ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<OrderResponse> GetOrderByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var order =
            await _dbContext
                .Orders()
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Order cannot be found!");

        return OrderDtoMapper.ToOrderResponse(order);
    }
}
