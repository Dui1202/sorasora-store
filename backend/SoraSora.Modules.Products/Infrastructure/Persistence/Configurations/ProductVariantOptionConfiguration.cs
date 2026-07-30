using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class ProductVariantOptionConfiguration
        : IEntityTypeConfiguration<ProductVariantOption>
    {
        public void Configure(EntityTypeBuilder<ProductVariantOption> builder)
        {
            builder.ToTable("ProductVariantOptions");

            builder.HasKey(x => new { x.ProductVariantId, x.VariationOptionId });
            builder.HasIndex(x => new { x.ProductVariantId, x.VariationOptionId }).IsUnique();
            builder.HasIndex(x => x.VariationOptionId);

            builder
                .HasOne(x => x.ProductVariant)
                .WithMany(x => x.ProductVariantOptions)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.VariationOption)
                .WithMany(x => x.ProductVariantOptions)
                .HasForeignKey(x => x.VariationOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
