namespace SoraSora.Infrastructure.Abstractions;

public interface IStorageCleanupProcessor
{
    Task ProcessPendingTasksAsync(CancellationToken cancellationToken);
}
