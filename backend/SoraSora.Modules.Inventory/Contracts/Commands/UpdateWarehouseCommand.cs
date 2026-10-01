namespace SoraSora.Modules.Inventory.Contracts.Commands;

public sealed record UpdateWarehouseCommand(string Id, string Name, bool IsDefault);
