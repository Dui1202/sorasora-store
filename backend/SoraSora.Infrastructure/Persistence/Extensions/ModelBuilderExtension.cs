using Microsoft.EntityFrameworkCore;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Persistence.Extensions
{
    public static class ModelBuilderExtension
    {
        public static void ApplyAuditConfiguration(this ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<IAuditable>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");
            modelBuilder
                .Entity<IAuditable>()
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        }
    }
}
