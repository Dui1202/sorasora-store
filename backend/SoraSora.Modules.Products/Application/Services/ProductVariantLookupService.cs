using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Infrastructure.Persistence.Extension;

public sealed class ProductVariantLookupService(SoraSoraDbContext dbContext)
    : IProductVariantLookupService
{
    public readonly SoraSoraDbContext _dbContext = dbContext;

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
    {
        return await _dbContext
            .ProductVariants()
            .AnyAsync(x => !x.IsArchived && x.Id == id, cancellationToken);
    }
}
