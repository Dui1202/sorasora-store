using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Inventory.Infrastructure.Persistence.Configurations;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryModuleModelBuilderContributor : IModelBuilderContributor
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryItemConfiguration).Assembly);
    }
}
