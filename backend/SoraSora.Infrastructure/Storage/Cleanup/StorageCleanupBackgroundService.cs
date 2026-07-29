using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoraSora.Infrastructure.Storage.Cleanup;

public sealed class StorageCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<StorageCleanupBackgroundService> logger
) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StorageCleanupBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process immediately on startup.
        await ProcessBatchSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessBatchSafelyAsync(stoppingToken);
        }
    }

    private async Task ProcessBatchSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var processor = scope.ServiceProvider.GetRequiredService<StorageCleanupProcessor>();

            await processor.ProcessPendingTasksAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Storage cleanup background processing failed");
        }
    }
}
