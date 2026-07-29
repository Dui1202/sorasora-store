using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Products.Application.Images;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService(
    SoraSoraDbContext dbContext,
    IFileStorageService fileStorageService,
    IProductImageProcessor productImageProcessor,
    ILogger<ProductService> logger
) : IProductService
{
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IProductImageProcessor _productImageProcessor = productImageProcessor;
    private readonly ILogger<ProductService> _logger = logger;
}
