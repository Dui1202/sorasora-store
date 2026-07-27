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

        builder
            .Property(x => x.Id)
            .HasValueGenerator<UlidStringValueGenerator>()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);
    }
}
