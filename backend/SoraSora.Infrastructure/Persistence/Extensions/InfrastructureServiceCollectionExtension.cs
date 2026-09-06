using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Persistence.Seeding;
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
        services.AddSingleton<DbSeederRunner>();

        services.AddDbContextFactory<SoraSoraDbContext>(
            (serviceProvider, options) =>
            {
                var seederRunner = serviceProvider.GetRequiredService<DbSeederRunner>();

                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .UseSeeding(
                        (context, _) =>
                        {
                            seederRunner.Seed((SoraSoraDbContext)context);
                        }
                    )
                    .UseAsyncSeeding(
                        async (context, _, cancellationToken) =>
                        {
                            await seederRunner.SeedAsync(
                                (SoraSoraDbContext)context,
                                cancellationToken
                            );
                        }
                    );
            }
        );

        services.AddSupabaseStorage(configuration);
        services.AddHostedService<StorageCleanupBackgroundService>();

        return services;
    }
}
