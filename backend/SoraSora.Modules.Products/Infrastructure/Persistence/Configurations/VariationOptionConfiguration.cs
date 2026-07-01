using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class VariationOptionConfiguration : IEntityTypeConfiguration<VariationOption>
    {
        public void Configure(EntityTypeBuilder<VariationOption> builder)
        {
            builder.ToTable("VariationOption");

            builder.HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .HasValueGenerator<UlidStringValueGenerator>()
                .ValueGeneratedOnAdd();
            builder.Property(x => x.VariationId).IsRequired();
            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.Position).IsRequired().HasDefaultValue(0);

            builder
                .HasOne(x => x.Variation)
                .WithMany(x => x.VariationOptions)
                .HasForeignKey(x => x.VariationId)
                .IsRequired();
        }
    }
}
