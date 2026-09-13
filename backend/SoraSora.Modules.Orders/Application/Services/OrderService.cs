using SoraSora.Infrastructure.Persistence;

namespace SoraSora.Modules.Orders.Application.Services;

public sealed partial class OrderService(SoraSoraDbContext dbContext) : IOrderService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
}
