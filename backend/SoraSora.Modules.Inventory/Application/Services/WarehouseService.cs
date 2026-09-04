using SoraSora.Infrastructure.Persistence;

public sealed partial class WarehouseService(SoraSoraDbContext dbContext) : IWarehouseService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;

    public Task CreateWarehouseAsync()
    {
        throw new NotImplementedException();
    }

    public Task DeleteWarehouseByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task GetAllWarehousesAsync()
    {
        throw new NotImplementedException();
    }

    public Task GetWarehouseByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateWarehouseAsync()
    {
        throw new NotImplementedException();
    }
}
