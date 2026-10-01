using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder
            .Property(x => x.StockMovementType)
            .HasConversion<string>()
            .HasDefaultValue(StockMovementType.Adjustment);

        builder.Property(x => x.OnHandedDelta).HasDefaultValue(0);
        builder.Property(x => x.ReservedDelta).HasDefaultValue(0);
        builder.Property(x => x.Description).IsRequired(false);

        builder
            .HasOne(x => x.Stock)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StockId);
    }
}
