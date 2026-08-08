using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Domain;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Infrastructure.Persistence.Extensions
{
    public static class MediaModuleDbContextExtension
    {
        public static DbSet<MediaAsset> MediaAssets(this SoraSoraDbContext dbContext)
        {
            return dbContext.Set<MediaAsset>();
        }
    }
}
