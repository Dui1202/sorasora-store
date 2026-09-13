using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Orders.Infrastructure.Persistence.Configurations;

namespace SoraSora.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersModuleModelBuilderContributor : IModelBuilderContributor
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderConfiguration).Assembly);
    }
}
