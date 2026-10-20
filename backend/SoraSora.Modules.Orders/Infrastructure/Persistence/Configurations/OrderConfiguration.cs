using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Version).ValueGeneratedNever().IsRequired().IsConcurrencyToken();
        builder
            .Property(x => x.CustomerName)
            .HasMaxLength(Order.MaxCustomerNameLength)
            .IsRequired();
        builder
            .Property(x => x.CustomerEmail)
            .HasMaxLength(Order.MaxCustomerEmailLength)
            .IsRequired(false);
        builder
            .Property(x => x.CustomerPhoneNumber)
            .HasMaxLength(Order.MaxCustomerPhoneNumberLength)
            .IsRequired();
        builder
            .Property(x => x.ShippingAddress)
            .HasMaxLength(Order.MaxShippingAddressLength)
            .IsRequired();
        builder
            .Property(x => x.CustomerNote)
            .HasMaxLength(Order.MaxCustomerNoteLength)
            .IsRequired(false);
        builder.Property(x => x.StoreNote).HasMaxLength(Order.MaxStoreNoteLength).IsRequired(false);
        builder
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(OrderStatus.Pending)
            .IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Ignore(x => x.TotalPrice);

        builder
            .HasMany(x => x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(x => x.OrderItems)
            .HasField("_orderItem")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.CreatedAt);
    }
}
