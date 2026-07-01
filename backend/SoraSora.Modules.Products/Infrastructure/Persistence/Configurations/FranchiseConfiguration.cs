using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Products.Domain;
using SoraSora.Shared.Helper;

namespace SoraSora.Modules.Products.Infrastructure.Persistence.Configurations
{
    public sealed class FranschiseConfiguration : IEntityTypeConfiguration<Franchise>
    {
        public void Configure(EntityTypeBuilder<Franchise> builder)
        {
            builder.ToTable("Franchise");
            builder.HasKey(x => x.Id);
            builder
                .Property(x => x.Id)
                .HasValueGenerator<UlidStringValueGenerator>()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Description).IsRequired(false);
            builder.Property(x => x.BannerUrl).IsRequired(false);
            builder.Property(x => x.LogoUrl).IsRequired(false);
            builder.Property(x => x.Slug).IsRequired(false);

            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Slug).IsUnique();
        }
    }
}
