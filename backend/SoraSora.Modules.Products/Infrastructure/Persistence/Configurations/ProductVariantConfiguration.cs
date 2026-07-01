using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariant");

            builder.HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<UlidStringValueGenerator>();
            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.CostPrice).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.SellingPrice).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.Sku).IsRequired();
            builder.HasIndex(x => x.Sku).IsUnique();
        }
    }
}
