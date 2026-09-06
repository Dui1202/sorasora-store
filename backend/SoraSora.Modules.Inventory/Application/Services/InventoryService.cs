using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;

public sealed partial class InventoryService(
    SoraSoraDbContext dbContext,
    IProductVariantLookupService productVariantLookupService
) : IInventoryService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IProductVariantLookupService _productVariantLookupService =
        productVariantLookupService;
}
