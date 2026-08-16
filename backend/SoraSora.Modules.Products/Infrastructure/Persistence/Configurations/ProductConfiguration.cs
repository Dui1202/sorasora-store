using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Version).IsRequired().ValueGeneratedNever().IsConcurrencyToken();

        builder.Property(x => x.Name).HasMaxLength(Product.MaxNameLength).IsRequired();
        builder
            .Property(x => x.Description)
            .HasMaxLength(Product.MaxDescriptionLength)
            .IsRequired(false);
        builder
            .Property(x => x.SupplierCountry)
            .HasConversion<string>()
            .HasDefaultValue(SupplierCountry.NONE);
        builder.Property(x => x.IsPublished).HasDefaultValue(false);
        builder.Property(x => x.IsArchived).HasDefaultValue(false);
        builder
            .Property(x => x.SellMode)
            .HasConversion<string>()
            .HasDefaultValue(SellMode.IN_STOCK);
    }
}
