using SoraSora.Infrastructure.Persistence;

public sealed partial class OrderService(SoraSoraDbContext dbContext) : IOrderService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
}
