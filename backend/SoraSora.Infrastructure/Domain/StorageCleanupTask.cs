using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Infrastructure.Domain;

public sealed class StorageCleanupTask : IAuditable
{
    private const int MaxRetryAttempts = 10;
    public const int MaxLastErrorLength = 2000;
    public const int MaxObjectPathLength = 4000;
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string ObjectPath { get; private set; } = null!;
    public StorageCleanupStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? LastError { get; private set; }

    private StorageCleanupTask(string objectPath)
    {
        ObjectPath = objectPath;
    }

    public static StorageCleanupTask Create(string objectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objectPath);

        return new StorageCleanupTask(objectPath);
    }

    public void MarkCompleted()
    {
        Status = StorageCleanupStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        LastError = null;
    }

    public void MarkFailed(string error, TimeSpan retryDelay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        AttemptCount++;
        LastError = error;
        NextAttemptAt = DateTimeOffset.UtcNow.Add(retryDelay);

        Status =
            AttemptCount >= MaxRetryAttempts
                ? StorageCleanupStatus.Failed
                : StorageCleanupStatus.Pending;
    }
}
