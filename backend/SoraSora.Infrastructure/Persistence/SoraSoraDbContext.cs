using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Persistence
{
    public sealed class SoraSoraDbContext(
        DbContextOptions<SoraSoraDbContext> options,
        IEnumerable<IModelBuilderContributor> modelBuilderContributors
    ) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                        entry.Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                        entry.Property(x => x.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Property(x => x.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
