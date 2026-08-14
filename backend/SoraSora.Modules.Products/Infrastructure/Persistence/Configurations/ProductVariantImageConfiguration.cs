using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantImageConfiguration : IEntityTypeConfiguration<ProductVariantImage>
{
    public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
    {
        builder.ToTable("ProductVariantImage");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ProductVariantId).IsRequired();
        builder.Property(x=>x.MediaAssetId).IsRequired();

        builder
            .HasOne(x => x.ProductVariant)
            .WithOne(x => x.ProductVariantImage)
            .HasForeignKey<ProductVariantImage>(x => x.ProductVariantId);

        builder.HasIndex(x=>x.MediaAssetId).IsUnique();
    }
}
