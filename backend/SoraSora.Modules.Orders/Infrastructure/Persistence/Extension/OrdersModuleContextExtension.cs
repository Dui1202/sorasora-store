using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Orders.Domain;

namespace SoraSora.Modules.Orders.Infrastructure.Persistence.Extension;

public static class OrdersModuleContextExtention
{
    public static DbSet<Order> Orders(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<Order>();
    }
}
