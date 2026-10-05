using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Media.Application;
using SoraSora.Modules.Products.Application.Services.Loggers;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService(
    SoraSoraDbContext dbContext,
    IImageMediaService imageMediaService,
    ILogger<ProductService> logger
) : IProductService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IImageMediaService _imageMediaService = imageMediaService;
    private readonly ILogger<ProductService> _logger = logger;

}
