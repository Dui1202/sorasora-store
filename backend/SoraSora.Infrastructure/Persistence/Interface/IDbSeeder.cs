namespace SoraSora.Infrastructure.Persistence.Interface;

public interface IDbSeeder
{
    void Seed(SoraSoraDbContext dbContext);

    Task SeedAsync(SoraSoraDbContext dbContext, CancellationToken cancellationToken);
}
