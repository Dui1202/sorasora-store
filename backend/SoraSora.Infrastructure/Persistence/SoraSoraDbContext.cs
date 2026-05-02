using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Products.Domain;

public sealed class SoraSoraDbContext(DbContextOptions<SoraSoraDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Franchise> Franchises => Set<Franchise>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Variation> Variations => Set<Variation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoraSoraDbContext).Assembly);
}