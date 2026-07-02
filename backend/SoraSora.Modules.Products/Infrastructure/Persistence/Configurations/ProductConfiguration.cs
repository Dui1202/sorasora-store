using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product");

            builder.HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<UlidStringValueGenerator>();

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Description).IsRequired(false);
            builder
                .Property(x => x.SupplierCountry)
                .HasConversion<string>()
                .HasDefaultValue(SupplierCountry.NONE);
            builder.Property(x => x.SellMode).HasConversion<string>().HasDefaultValue("IN_STOCK");
            builder.Property(x => x.PreorderReleaseAt).IsRequired(false);
            builder.Property(x => x.IsPublished).HasDefaultValue(false);
            builder.Property(x => x.HasVariation).HasDefaultValue(false);
            builder.Property(x => x.IsArchived).HasDefaultValue(false);

            builder.HasMany(x => x.ProductVariants).WithOne(x => x.Product);
            builder.HasMany(x => x.Variations).WithOne(x => x.Product);
        }
    }
}
