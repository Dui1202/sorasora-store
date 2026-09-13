namespace SoraSora.Modules.Orders.Contracts.Responses;

public sealed record OrderResponse(
    string Id,
    IReadOnlyCollection<OrderItemResponse> OrderItems,
    int TotalPrice,
    string CustomerName,
    string? CustomerEmail,
    string CustomerPhoneNumber,
    string ShippingAddress,
    OrderStatus Status,
    string? CustomerNote,
    string? StoreNote,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
