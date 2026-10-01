using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Domain;
using SoraSora.Modules.Inventory.Infrastructure.Persistence.Extension;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Seeding;

public sealed class WarehouseSeeder : IDbSeeder
{
    public void Seed(SoraSoraDbContext dbContext)
    {
        if (dbContext.Warehouses().Any(x => x.IsDefault))
        {
            return;
        }

        var warehouse = Warehouse.Create("Default");
        warehouse.SetDefault(true);

        dbContext.Warehouses().Add(warehouse);
        dbContext.SaveChanges();
    }

    public async Task SeedAsync(SoraSoraDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Warehouses().AnyAsync(x => x.IsDefault, cancellationToken))
        {
            return;
        }

        var warehouse = Warehouse.Create("Default");
        warehouse.SetDefault(true);

        dbContext.Warehouses().Add(warehouse);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
