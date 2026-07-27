using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

namespace SoraSora.Modules.Products.Infrastructure.Persistence;

public sealed class ProductsModuleModelBuilderContributor : IModelBuilderContributor
{
    public void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
    }
}
