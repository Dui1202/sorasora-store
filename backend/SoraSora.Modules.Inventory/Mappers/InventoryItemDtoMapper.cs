using SoraSora.Modules.Inventory.Contracts.Responds;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Mappers;

public static class InventoryItemDtoMapper
{
    public static InventoryItemResponse ToInventoryItemResponse(InventoryItem inventoryItem)
    {
        return new InventoryItemResponse(
            Id: inventoryItem.Id,
            ProductVariantId: inventoryItem.ProductVariantId,
            Stocks: inventoryItem.Stocks.Select(x => ToStockResponse(x)).ToArray()
        );
    }

    public static StockResponse ToStockResponse(Stock stock)
    {
        return new StockResponse(
            WarehouseId: stock.WarehouseId,
            OnHandedQuantity: stock.OnHandedQuantity,
            ReservedQuantity: stock.ReservedQuantity
        );
    }
}
