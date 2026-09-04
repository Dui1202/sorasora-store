namespace SoraSora.Modules.Inventory.Application.Services;

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryItemResponse>> GetAllInventoryItemsAsync(
        CancellationToken cancellationToken
    );
    Task<InventoryItemResponse> GetInventoryItemByIdAsync(
        string id,
        CancellationToken cancellationToken
    );
    Task<InventoryItemResponse> CreateInventoryItemAsync(
        CreateInventoryCommand command,
        CancellationToken cancellationToken
    );
}
