using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Storage;
using SoraSora.Infrastructure.Storage.Cleanup;

namespace SoraSora.Infrastructure.Persistence.Extensions;

public static class InfrastructureServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<SoraSoraDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );
        services.AddSupabaseStorage(configuration);
        services.AddScoped<StorageCleanupProcessor>();

        services.AddHostedService<StorageCleanupBackgroundService>();

        return services;
    }
}
