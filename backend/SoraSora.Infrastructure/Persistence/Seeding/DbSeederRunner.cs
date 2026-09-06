using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Persistence.Interface;

namespace SoraSora.Infrastructure.Persistence.Seeding;

public sealed class DbSeederRunner(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void Seed(SoraSoraDbContext dbContext)
    {
        using var scope = _serviceProvider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<IDbSeeder>().ToList();

        foreach (var seeder in seeders)
        {
            seeder.Seed(dbContext);
        }
    }

    public async Task SeedAsync(SoraSoraDbContext dbContext, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var seeders = scope.ServiceProvider.GetServices<IDbSeeder>().ToList();

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(dbContext, cancellationToken);
        }
    }
}
