using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable(
            "ProductVariant",
            table =>
            {
                table.HasCheckConstraint("CK_ProductVariant_CostPrice", "\"CostPrice\" >= 0");
                table.HasCheckConstraint("CK_ProductVariant_SellingPrice", "\"SellingPrice\" >= 0");
                table.HasCheckConstraint(
                    "CK_ProductVariant_Position",
                    "(\"IsArchived\" AND \"Position\" IS NULL) OR "
                        + "(NOT \"IsArchived\" AND \"Position\" IS NOT NULL AND \"Position\" >= 0)"
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ClientId).ValueGeneratedNever();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(ProductVariant.MaxNameLength).IsRequired();
        builder.Property(x => x.Sku);
        builder.Property(x => x.CostPrice).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.SellingPrice).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.IsArchived).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Position).IsRequired(false);

        builder
            .HasOne(v => v.Product)
            .WithMany(p => p.ProductVariants)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new { x.ProductId, x.Position })
            .IsUnique()
            .HasFilter("\"IsArchived\" = false");
        ;
        builder.HasIndex(x => x.ClientId).IsUnique();
    }
}
