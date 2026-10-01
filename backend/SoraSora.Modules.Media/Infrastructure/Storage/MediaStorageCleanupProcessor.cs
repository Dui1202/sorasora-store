using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Media.Infrastructure.Persistence;

namespace SoraSora.Modules.Media.Infrastructure.Storage;

public sealed class MediaStorageCleanupProcessor(
    SoraSoraDbContext dbContext,
    IFileStorageService fileStorageService,
    ILogger<MediaStorageCleanupProcessor> logger
) : IStorageCleanupProcessor
{
    private const int BatchSize = 25;
    private const int MaxRetryDelayMinutes = 60;
    private readonly SoraSoraDbContext _dbContext = dbContext;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<MediaStorageCleanupProcessor> _logger = logger;

    public async Task ProcessPendingTasksAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var cleanupTasks = await _dbContext
            .StorageCleanupTasks()
            .Where(task => task.Status == StorageCleanupStatus.Pending && task.NextAttemptAt <= now)
            .OrderBy(task => task.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var task in cleanupTasks)
        {
            await ProcessTaskAsync(task, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessTaskAsync(
        StorageCleanupTask task,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _fileStorageService.DeleteFileAsync(task.ObjectPath, cancellationToken);

            var mediaAsset = await _dbContext
                .MediaAssets()
                .SingleOrDefaultAsync(asset => asset.Path == task.ObjectPath, cancellationToken);

            if (mediaAsset is not null)
            {
                _dbContext.MediaAssets().Remove(mediaAsset);
            }

            task.MarkCompleted();

            _logger.LogInformation(
                "Deleted storage object {ObjectPath} and matching media metadata {MediaAssetId}",
                task.ObjectPath,
                mediaAsset?.Id
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var retryDelay = CalculateRetryDelay(task.AttemptCount + 1);

            task.MarkFailed(LimitErrorMessage(exception.Message), retryDelay);

            _logger.LogWarning(
                exception,
                "Failed to clean up storage object {ObjectPath}. "
                    + "Attempt {AttemptCount}; next attempt at {NextAttemptAt}",
                task.ObjectPath,
                task.AttemptCount,
                task.NextAttemptAt
            );
        }
    }

    private static TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var minutes = Math.Min(Math.Pow(2, attemptCount - 1), MaxRetryDelayMinutes);

        return TimeSpan.FromMinutes(minutes);
    }

    private static string LimitErrorMessage(string message)
    {
        return message.Length <= StorageCleanupTask.MaxLastErrorLength
            ? message
            : message[..StorageCleanupTask.MaxLastErrorLength];
    }
}
