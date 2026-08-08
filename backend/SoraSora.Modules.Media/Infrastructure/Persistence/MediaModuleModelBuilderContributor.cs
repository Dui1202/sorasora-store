using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Media.Infrastructure.Persistence.Configurations;

namespace SoraSora.Modules.Media.Infrastructure.Persistence
{
    public sealed class MediaModuleModelBuilderContributor : IModelBuilderContributor
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaAssetConfiguration).Assembly);
        }
    }
}
