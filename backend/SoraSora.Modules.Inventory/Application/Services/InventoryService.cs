using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService(SoraSoraDbContext dbContext) : IInventoryService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
}
