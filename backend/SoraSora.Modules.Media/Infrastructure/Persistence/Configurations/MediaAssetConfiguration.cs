using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Media.Infrastructure.Persistence.Configurations
{
    public sealed class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
    {
        public void Configure(EntityTypeBuilder<MediaAsset> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.HasIndex(x => x.Path).IsUnique();
            builder.Property(x => x.Path).IsRequired();

            builder.Property(x => x.ContentType).IsRequired();
            builder.Property(x => x.Alt).HasMaxLength(MediaAsset.MaxAltLength).IsRequired(false);
        }
    }
}
