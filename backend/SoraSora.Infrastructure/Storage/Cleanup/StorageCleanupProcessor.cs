using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Infrastructure.Persistence.Extensions;

namespace SoraSora.Infrastructure.Storage.Cleanup
{
    public sealed class StorageCleanupProcessor(
        SoraSoraDbContext dbContext,
        IFileStorageService fileStorageService,
        ILogger<StorageCleanupProcessor> logger
    )
    {
        private const int BatchSize = 25;
        private const int MaxRetryDelayMinutes = 60;
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<StorageCleanupProcessor> _logger = logger;

        public async Task ProcessPendingTasksAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var cleanupTasks = await _dbContext
                .StorageCleanupTasks()
                .Where(x => x.Status == StorageCleanupStatus.Pending && x.NextAttemptAt <= now)
                .OrderBy(x => x.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            foreach (var task in cleanupTasks)
            {
                await ProcessTaskAsync(task, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ProcessTaskAsync(
            StorageCleanupTask task,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(task.ObjectPath, cancellationToken);

                task.MarkCompleted();

                _logger.LogInformation(
                    "Deleted orphaned storage object {ObjectPath}",
                    task.ObjectPath
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
                    "Failed to delete storage object {ObjectPath}. "
                        + "Attempt {AttemptCount}; next attempt at {NextAttemptAt}",
                    task.ObjectPath,
                    task.AttemptCount,
                    task.NextAttemptAt
                );
            }
        }

        private static TimeSpan CalculateRetryDelay(int attemptCount)
        {
            // 1, 2, 4, 8, 16, 32, 60... minutes
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
}
