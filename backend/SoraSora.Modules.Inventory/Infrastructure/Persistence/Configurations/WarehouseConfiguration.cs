using SoraSora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SoraSora.Modules.Inventory.Domain;

namespace SoraSora.Modules.Inventory.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(Warehouse.MaxNameLength).IsRequired();
        builder.Property(x => x.IsDefault).HasDefaultValue(false).IsRequired();
        builder.Property(x => x.IsArchive).HasDefaultValue(false).IsRequired();

        builder
            .HasIndex(x => x.Name)
            .HasDatabaseName(DatabaseConstraintNames.Warehouses.Name)
            .IsUnique();

        builder
            .HasIndex(x => x.IsDefault)
            .HasDatabaseName(DatabaseConstraintNames.Warehouses.SingleDefault)
            .IsUnique()
            .HasFilter("\"IsDefault\" = TRUE");
    }
}
