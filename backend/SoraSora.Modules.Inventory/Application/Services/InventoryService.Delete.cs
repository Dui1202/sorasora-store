using SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService : IInventoryService
{
    public async Task DeleteInventoryItemAsync(
        string id,
        CancellationToken cancellationToken = default
    ) { }
}
