namespace SoraSora.Modules.Inventory.Contracts.Responds;

public sealed record WarehouseResponse(string Id, string Name, bool IsDefault, bool IsArchive);
