using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;

public static class OrdersModuleContextExtention
{
    public static DbSet<Order> Orders(this SoraSoraDbContext dbContext)
    {
        return dbContext.Set<Order>();
    }
}
