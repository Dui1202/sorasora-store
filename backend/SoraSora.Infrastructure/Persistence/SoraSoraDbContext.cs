using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Persistence
{
    public sealed class SoraSoraDbContext(
        DbContextOptions<SoraSoraDbContext> options,
        IEnumerable<IModelBuilderContributor> modelBuilderContributors
    ) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyAuditConfiguration();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SoraSoraDbContext).Assembly);
            foreach (var contributor in modelBuilderContributors)
            {
                contributor.Configure(modelBuilder);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
