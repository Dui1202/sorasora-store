using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Contracts.Responses;

public sealed record OrderItemResponse(
    string Id,
    int SellingPrice,
    string OrderId,
    string ProductVariantId,
    int Quantity,
    OrderSaleType SaleType,
    string WarehouseId
);
