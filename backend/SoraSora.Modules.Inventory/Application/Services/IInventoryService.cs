using SoraSora.Modules.Inventory.Contracts.Commands;

namespace SoraSora.Modules.Inventory.Application.Services;

public interface IInventoryService
{
    Task CreateStocks(IReadOnlyCollection<string> productVariantIds);
    Task AddStocks(IReadOnlyCollection<AddStockCommand> commands);
    Task ReservedStocks(IReadOnlyCollection<string> commands);
}
