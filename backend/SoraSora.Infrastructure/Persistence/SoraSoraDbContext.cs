using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Persistence
{
    public sealed class SoraSoraDbContext(DbContextOptions<SoraSoraDbContext> options)
        : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Franchise> Franchises => Set<Franchise>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Variation> Variations => Set<Variation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ModelBuilderExtension.ApplyAuditConfiguration(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoraSoraDbContext).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
