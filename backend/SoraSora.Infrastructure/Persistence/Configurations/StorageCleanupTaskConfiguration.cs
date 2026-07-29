using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoraSora.Infrastructure.Domain;

namespace SoraSora.Infrastructure.Persistence.Configurations;

public sealed class StorageCleanupTaskConfiguration : IEntityTypeConfiguration<StorageCleanupTask>
{
    public void Configure(EntityTypeBuilder<StorageCleanupTask> builder)
    {
        builder.ToTable("StorageCleanupTasks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder
            .Property(x => x.ObjectPath)
            .HasMaxLength(StorageCleanupTask.MaxObjectPathLength)
            .IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(StorageCleanupStatus.Pending)
            .IsRequired();

        builder.Property(x => x.LastError).HasMaxLength(StorageCleanupTask.MaxLastErrorLength);

        builder.Property(x => x.AttemptCount).IsRequired();

        builder.Property(x => x.NextAttemptAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });

        builder.HasIndex(x => x.ObjectPath).IsUnique();
    }
}
