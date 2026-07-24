using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Auth.Infrastructure.Persistence.Configurations;

namespace SoraSora.Modules.Auth.Infrastructure.Persistence
{
    public sealed class AuthModuleModelBuilderContributor : IModelBuilderContributor
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(IdentityUserRoleConfiguration).Assembly
            );
        }
    }
}
