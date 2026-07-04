using Microsoft.EntityFrameworkCore;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Persistence.Extensions
{
    public static class ModelBuilderExtension
    {
        public static void ApplyAuditConfiguration(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IAuditable.CreatedAt))
                    .HasDefaultValueSql("NOW()");

                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IAuditable.UpdatedAt))
                    .HasDefaultValueSql("NOW()");
            }
        }
    }
}
