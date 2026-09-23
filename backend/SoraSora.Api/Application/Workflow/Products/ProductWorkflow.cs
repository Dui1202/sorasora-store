using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Media.Application;
using SoraSora.Modules.Products.Application.Services;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow(
    SoraSoraDbContext dbContext,
    IProductService productService,
    IInventoryService inventoryService,
    IImageMediaService imageMediaService,
    ILogger<ProductWorkflow> logger
)
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IProductService _productService = productService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IImageMediaService _imageMediaService = imageMediaService;
    private readonly ILogger<ProductWorkflow> _logger = logger;
}
