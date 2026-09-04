using Microsoft.AspNetCore.Mvc.Diagnostics;
using SoraSora.Modules.Inventory.Domain;

public static class InventoryItemDtoMapper
{
    public static InventoryItemResponse ToInventoryItemResponse(InventoryItem inventoryItem)
    {
        return new InventoryItemResponse(
            ProductVariantId: inventoryItem.ProductVariantId,
            Stocks: inventoryItem.Stocks.Select(x => ToStockResponse(x)).ToArray()
        );
    }

    private static StockResponse ToStockResponse(Stock stock)
    {
        return new StockResponse(
            WarehouseId: stock.WarehouseId,
            OnHandedQuantity: stock.OnHandedQuantity,
            ReservedQuantity: stock.ReservedQuantity
        );
    }
}
