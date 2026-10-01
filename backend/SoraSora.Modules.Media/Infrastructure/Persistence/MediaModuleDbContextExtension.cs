using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Media.Infrastructure.Persistence
{
    public static class MediaModuleDbContextExtension
    {
        public static DbSet<MediaAsset> MediaAssets(this SoraSoraDbContext dbContext)
        {
            return dbContext.Set<MediaAsset>();
        }
    }
}
