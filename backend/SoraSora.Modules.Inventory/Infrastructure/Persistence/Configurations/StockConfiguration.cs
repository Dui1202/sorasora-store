using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Inventory.Domain;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.InventoryItemId).IsRequired();
        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.OnHandedQuantity).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ReservedQuantity).IsRequired().HasDefaultValue();
        builder.Property(x => x.StockMovements);

        builder
            .HasOne(x => x.InventoryItem)
            .WithMany(x => x.Stocks)
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.InventoryItemId, x.WarehouseId }).IsUnique();
    }
}
