using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "OrderItems",
            table =>
            {
                table.HasCheckConstraint(
                    DatabaseConstraintNames.OrderItems.SellingPrice,
                    "\"SellingPrice\" >= 0"
                );
                table.HasCheckConstraint(
                    DatabaseConstraintNames.OrderItems.Quantity,
                    "\"Quantity\" > 0"
                );
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.WarehouseId).IsRequired();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ProductVariantId).IsRequired();
        builder.Property(x => x.SellingPrice).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.SaleType).HasConversion<string>().IsRequired();

        builder.HasIndex(x => x.OrderId);
    }
}
