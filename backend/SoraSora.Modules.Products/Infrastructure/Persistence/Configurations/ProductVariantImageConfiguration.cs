using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class ProductVariantImageConfiguration
        : IEntityTypeConfiguration<ProductVariantImage>
    {
        public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
        {
            builder.ToTable("ProductVariantImage");

            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasValueGenerator<UlidStringValueGenerator>()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ProductVariantId).IsRequired();
            builder.Property(x => x.Path).IsRequired();
            builder.Property(x => x.Alt).IsRequired(false);

            builder
                .HasOne(x => x.ProductVariant)
                .WithOne(x => x.Image)
                .HasForeignKey<ProductVariantImage>(x => x.ProductVariantId)
                .IsRequired();
        }
    }
}
