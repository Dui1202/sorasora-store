namespace SoraSora.Modules.Inventory.Contracts.Requests;

public sealed record UpdateWarehouseRequest(string Id, string Name, bool IsDefault);
