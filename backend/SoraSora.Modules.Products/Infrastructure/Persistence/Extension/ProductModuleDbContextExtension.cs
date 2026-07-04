using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Extension
{
    public static class ProductModuleDbContextExtension
    {
        public static DbSet<Product> Products(this SoraSoraDbContext dbContext)
        {
            return dbContext.Set<Product>();
        }

        public static DbSet<ProductVariant> ProductVariants(this SoraSoraDbContext dbContext)
        {
            return dbContext.Set<ProductVariant>();
        }
    }
}
