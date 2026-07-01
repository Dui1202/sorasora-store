using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImage");

            builder.HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .HasValueGenerator<UlidStringValueGenerator>()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ProductId).IsRequired();
            builder.Property(x => x.Path).IsRequired();
            builder.Property(x => x.Alt).IsRequired(false);
            builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);

            builder
                .HasOne(x => x.Product)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.ProductId)
                .IsRequired();

            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => new { x.ProductId, x.Position }).IsUnique();
        }
    }
}
