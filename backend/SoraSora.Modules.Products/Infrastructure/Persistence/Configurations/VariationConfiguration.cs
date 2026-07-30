using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations;

public sealed class VariationConfiguration : IEntityTypeConfiguration<Variation>
{
    public void Configure(EntityTypeBuilder<Variation> builder)
    {
        builder.ToTable("Variation");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ProductId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.Position });

        builder
            .Property(x => x.Id)
            .HasValueGenerator<UlidStringValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.Variations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
