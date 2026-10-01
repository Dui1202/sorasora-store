using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Extension;

public static class InventoryModuleContextExtention
{
    public static DbSet<InventoryItem> InventoryItems(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<InventoryItem>();
    }

    public static DbSet<Warehouse> Warehouses(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<Warehouse>();
    }
}
