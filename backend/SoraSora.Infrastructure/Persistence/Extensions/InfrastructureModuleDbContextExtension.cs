using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;

namespace SoraSora.Infrastructure.Persistence.Extensions
{
    public static class InfrastructureModuleDbContextExtension
    {
        public static DbSet<StorageCleanupTask> StorageCleanupTasks(
            this SoraSoraDbContext dbContext
        )
        {
            return dbContext.Set<StorageCleanupTask>();
        }
    }
}
