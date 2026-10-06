using SoraSora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable(
            "Stocks",
            table =>
            {
                table.HasCheckConstraint(DatabaseConstraintNames.Stocks.OnHandedQuantity, "\"OnHandedQuantity\" >= 0");
                table.HasCheckConstraint(DatabaseConstraintNames.Stocks.ReservedQuantity, "\"ReservedQuantity\" >= 0");
            }
        );

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.InventoryItemId).IsRequired();
        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.OnHandedQuantity).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ReservedQuantity).IsRequired().HasDefaultValue(0);

        builder
            .HasOne(x => x.InventoryItem)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Warehouse)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.WarehouseId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.WarehouseId);
        builder.HasIndex(x => new { x.InventoryItemId, x.WarehouseId }).IsUnique();
    }
}
