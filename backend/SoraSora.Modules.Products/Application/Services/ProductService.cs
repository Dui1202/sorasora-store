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

    private async Task TryScheduleUploadedMediaCleanupAsync(
        IReadOnlyCollection<string> uploadedMediaAssetIds
    )
    {
        if (uploadedMediaAssetIds.Count == 0)
        {
            return;
        }

        try
        {
            // Creation can participate in a workflow transaction. End it before saving
            // cleanup tasks, otherwise the caller's rollback would discard those tasks too.
            if (_dbContext.Database.CurrentTransaction is { } transaction)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                await transaction.DisposeAsync();
            }

            // A failed save or rolled-back transaction leaves tracked changes behind. Clear them
            // so saving cleanup tasks cannot accidentally persist the failed product graph.
            _dbContext.ChangeTracker.Clear();

            await _imageMediaService.ScheduleImageDeletionAsync(
                uploadedMediaAssetIds,
                CancellationToken.None
            );

            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception cleanupException)
        {
            foreach (var mediaAssetId in uploadedMediaAssetIds)
            {
                _logger.ProductImageCleanupFailed(cleanupException, mediaAssetId);
            }
        }
    }
}
