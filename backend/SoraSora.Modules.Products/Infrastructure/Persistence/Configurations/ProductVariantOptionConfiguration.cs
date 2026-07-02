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
            builder.ToTable("ProductVariantOption");

            builder.HasKey(x => new { x.ProductVariantId, x.VariationId });
            builder.HasIndex(x => new { x.ProductVariantId, x.VariationOptionId }).IsUnique();

            builder.Property(x => x.ProductVariantId).IsRequired();
            builder.Property(x => x.VariationId).IsRequired();
            builder.Property(x => x.VariationOptionId).IsRequired();

            builder
                .HasOne(x => x.Variation)
                .WithMany(x => x.ProductVariantOptions)
                .HasForeignKey(x => x.VariationId)
                .IsRequired();

            builder
                .HasOne(x => x.VariationOption)
                .WithMany(x => x.ProductVariantOptions)
                .HasForeignKey(x => new { x.VariationOptionId, x.VariationId })
                .HasPrincipalKey(x => new { x.Id, x.VariationId });
        }
    }
}
