namespace SoraSora.Modules.Inventory.Contracts.Responds;

public sealed record StockResponse(string WarehouseId, int OnHandedQuantity, int ReservedQuantity);
