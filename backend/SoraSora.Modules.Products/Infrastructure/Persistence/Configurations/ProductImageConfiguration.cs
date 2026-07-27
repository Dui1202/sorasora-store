using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable(
            "ProductImage",
            table => table.HasCheckConstraint("CK_ProductImage_Position", "\"Position\" >= 0")
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Alt).HasMaxLength(255).IsRequired(false);
        builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);
        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.ProductImages)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ProductId, x.Position }).IsUnique();
    }
}
