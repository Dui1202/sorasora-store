using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

public static class ProductsModuleDbContextExtension
{
    public static DbSet<Product> Products(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<Product>();
    }

    public static DbSet<ProductVariant> ProductVariants(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<ProductVariant>();
    }

    public static DbSet<Variation> Variations(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<Variation>();
    }

    public static DbSet<VariationOption> VariationOptions(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<VariationOption>();
    }
}
