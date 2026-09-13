using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;

namespace SoraSora.Modules.Orders.Application.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken
        );

        Task<OrderResponse> CancelOrderAsync(string orderId, CancellationToken cancellationToken);
        Task<OrderResponse> CompleteOrderAsync(string orderId, CancellationToken cancellationToken);
        Task<OrderResponse> UpdateOrderAsync(
            UpdateOrderCommand command,
            CancellationToken cancellationToken
        );
        Task<OrderResponse> GetOrderByIdAsync(string id, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<OrderResponse>?> GetAllOrdersAsync(
            CancellationToken cancellationToken
        );
    }
}
