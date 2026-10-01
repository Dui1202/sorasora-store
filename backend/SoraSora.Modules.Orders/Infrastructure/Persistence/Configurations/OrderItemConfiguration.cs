using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
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
                table.HasCheckConstraint("CK_OrderItems_SellingPrice", "\"SellingPrice\" >= 0");
                table.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
            }
        );

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ProductVariantId).IsRequired();
        builder.Property(x => x.SellingPrice).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();

        builder.HasIndex(x => x.OrderId);
    }
}
