using SoraSora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.IsArchived).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.ProductVariantId).IsRequired();

        builder
            .HasIndex(x => x.ProductVariantId)
            .IsUnique()
            .HasDatabaseName(DatabaseConstraintNames.InventoryItems.ProductVariantId);
        ;
    }
}
