using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Media.Infrastructure;

namespace SoraSora.Modules.Media.Application
{
    public class ImageMediaService(
        SoraSoraDbContext dbContext,
        IDbContextFactory<SoraSoraDbContext> dbContextFactory,
        IImageProcessor imageProcessor,
        IFileStorageService fileStorageService,
        ILogger<ImageMediaService> logger
    ) : IImageMediaService
    {
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly IDbContextFactory<SoraSoraDbContext> _dbContextFactory = dbContextFactory;
        private readonly IImageProcessor _imageProcessor = imageProcessor;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<ImageMediaService> _logger = logger;

        private const int UploadWorkerCount = 2;
        private const int MaxBufferedImages = 2;

        public async Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyList<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        )
        {
            ConcurrentDictionary<string, byte> attemptedUploadPaths = new(StringComparer.Ordinal);
            ConcurrentBag<MediaUpload> successfulUploads = [];

            var channel = Channel.CreateBounded<ProcessedImageUpload>(
                new BoundedChannelOptions(MaxBufferedImages)
                {
                    SingleWriter = true,
                    SingleReader = UploadWorkerCount == 1,
                    FullMode = BoundedChannelFullMode.Wait,
                    AllowSynchronousContinuations = false,
                }
            );

            using var bufferedImageSlots = new SemaphoreSlim(MaxBufferedImages, MaxBufferedImages);

            using var pipelineCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken
            );

            var pipelineToken = pipelineCancellation.Token;

            var pipelineFailures = new ConcurrentQueue<Exception>();

            var consumers = Enumerable
                .Range(0, UploadWorkerCount)
                .Select(_ =>
                    ConsumeProcessedImageAsync(
                        channel.Reader,
                        bufferedImageSlots,
                        attemptedUploadPaths,
                        successfulUploads,
                        pipelineFailures,
                        pipelineCancellation,
                        pipelineToken
                    )
                )
                .ToArray();

            var producers = ProduceProcessedImagesAsync(
                jobs,
                channel.Writer,
                bufferedImageSlots,
                pipelineFailures,
                pipelineCancellation,
                pipelineToken
            );

            var pipelineTasks = consumers.Append(producers).ToArray();

            try
            {
                await Task.WhenAll(pipelineTasks);

                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                    cancellationToken
                );
                var uploadResults = new List<MediaUploadResult>(successfulUploads.Count);

                foreach (var upload in successfulUploads)
                {
                    var mediaAsset = MediaAsset.Create(upload.Path, upload.ContentType);
                    uploadResults.Add(new MediaUploadResult(upload.Job.Id, mediaAsset.Id));

                    dbContext.MediaAssets().Add(mediaAsset);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return uploadResults;
            }
            catch
            {
                await ScheduleFailedUploadCleanupAsync(attemptedUploadPaths.Keys.ToArray());

                // Prefer the request cancellation when it caused the pipeline failure.
                cancellationToken.ThrowIfCancellationRequested();

                if (pipelineFailures.TryPeek(out var pipelineFailure))
                {
                    ExceptionDispatchInfo.Capture(pipelineFailure).Throw();
                }

                throw;
            }
        }

        public async Task DeleteImagesAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        )
        {
            var requestedAssetIds = mediaAssetIds
                .Where(mediaAssetId => !string.IsNullOrWhiteSpace(mediaAssetId))
                .ToHashSet(StringComparer.Ordinal);

            if (requestedAssetIds.Count == 0)
            {
                return;
            }

            try
            {
                var mediaAssets = await _dbContext
                    .MediaAssets()
                    .Where(asset => requestedAssetIds.Contains(asset.Id))
                    .ToListAsync(cancellationToken);
                var failedDeletionPaths = new HashSet<string>(StringComparer.Ordinal);

                foreach (var mediaAsset in mediaAssets)
                {
                    _dbContext.MediaAssets().Remove(mediaAsset);

                    try
                    {
                        await _fileStorageService.DeleteFileAsync(
                            mediaAsset.Path,
                            cancellationToken
                        );
                    }
                    catch (Exception exception)
                    {
                        failedDeletionPaths.Add(mediaAsset.Path);
                        _logger.LogWarning(
                            exception,
                            "Failed to delete media asset {MediaAssetId} at {ObjectPath}; scheduling cleanup",
                            mediaAsset.Id,
                            mediaAsset.Path
                        );
                    }
                }

                await AddCleanupTasksAsync(
                    _dbContext,
                    failedDeletionPaths,
                    cancellationToken
                );
            }
            catch
            {
                await ScheduleImageDeletionAsync(requestedAssetIds, CancellationToken.None);
            }
        }

        public async Task ScheduleImageDeletionAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        )
        {
            var requestedAssetIds = mediaAssetIds
                .Where(mediaAssetId => !string.IsNullOrWhiteSpace(mediaAssetId))
                .ToHashSet(StringComparer.Ordinal);

            if (requestedAssetIds.Count == 0)
            {
                return;
            }

            var mediaAssetPathsById = await _dbContext
                .MediaAssets()
                .Where(asset => requestedAssetIds.Contains(asset.Id))
                .ToDictionaryAsync(asset => asset.Id, asset => asset.Path, cancellationToken);

            if (mediaAssetPathsById.Count != requestedAssetIds.Count)
            {
                var missingAssetIds = requestedAssetIds
                    .Where(assetId => !mediaAssetPathsById.ContainsKey(assetId))
                    .ToArray();

                _logger.LogWarning(
                    "Could not schedule image cleanup because media metadata was missing for asset IDs {MediaAssetIds}",
                    missingAssetIds
                );
            }

            await AddCleanupTasksAsync(_dbContext, mediaAssetPathsById.Values, cancellationToken);
        }

        private async Task ScheduleFailedUploadCleanupAsync(
            IReadOnlyCollection<string> attemptedUploadPaths
        )
        {
            if (attemptedUploadPaths.Count == 0)
            {
                return;
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                    CancellationToken.None
                );

                await AddCleanupTasksAsync(
                    dbContext,
                    attemptedUploadPaths,
                    CancellationToken.None
                );
                await dbContext.SaveChangesAsync(CancellationToken.None);

                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to schedule cleanup for uploaded objects {ObjectPaths}; falling back to direct deletion",
                    attemptedUploadPaths
                );
            }

            await DeleteUnscheduledUploadsAsync(attemptedUploadPaths);
        }

        private async Task ProduceProcessedImagesAsync(
            IReadOnlyList<ImageUploadJob> jobs,
            ChannelWriter<ProcessedImageUpload> writer,
            SemaphoreSlim bufferedImageSlots,
            ConcurrentQueue<Exception> failures,
            CancellationTokenSource pipelineCancellation,
            CancellationToken cancellationToken
        )
        {
            try
            {
                foreach (var job in jobs)
                {
                    await bufferedImageSlots.WaitAsync(cancellationToken);

                    //producer owns the semaphore permit
                    var handedToConsumer = false;

                    try
                    {
                        await using var source = job.File.OpenReadStream();

                        var processedImage = _imageProcessor.Process(
                            source,
                            job.File.Length,
                            cancellationToken
                        );

                        var path = job.BuildPath(
                            ImageContentType.ToImageContentType(processedImage.Format)
                        );

                        await writer.WriteAsync(
                            new ProcessedImageUpload(job, processedImage, path),
                            cancellationToken
                        );

                        //After the producer successfully writes the item into the channel,
                        //now the consumer will own the permit, and handle the Release() call instead of producer
                        handedToConsumer = true;
                    }
                    finally
                    {
                        //Producer handles the permit and call Release()
                        if (!handedToConsumer)
                            bufferedImageSlots.Release();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Enqueue(exception);
                pipelineCancellation.Cancel();

                throw;
            }
            finally
            {
                //Close the write channel
                writer.TryComplete();
            }
        }

        private async Task ConsumeProcessedImageAsync(
            ChannelReader<ProcessedImageUpload> reader,
            SemaphoreSlim bufferedImageSlots,
            ConcurrentDictionary<string, byte> attemptedUploadPaths,
            ConcurrentBag<MediaUpload> successfulUploads,
            ConcurrentQueue<Exception> failures,
            CancellationTokenSource pipelineCancellation,
            CancellationToken cancellationToken
        )
        {
            try
            {
                await foreach (var processedImage in reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        var path = processedImage.Path;
                        var contentType = processedImage.Image.ContentType;
                        var job = processedImage.ImageUploadJob;

                        if (!attemptedUploadPaths.TryAdd(path, 0))
                        {
                            throw new InvalidOperationException(
                                $"Duplicate media upload path detected: {path}"
                            );
                        }

                        await using var content = new MemoryStream(
                            processedImage.Image.Content,
                            writable: false
                        );

                        await _fileStorageService.UploadFileAsync(
                            content,
                            path,
                            processedImage.Image.ContentType,
                            cancellationToken
                        );

                        successfulUploads.Add(new MediaUpload(job, path, contentType));
                    }
                    finally
                    {
                        bufferedImageSlots.Release();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Enqueue(exception);
                pipelineCancellation.Cancel();

                throw;
            }
        }

        private static async Task AddCleanupTasksAsync(
            SoraSoraDbContext dbContext,
            IEnumerable<string> objectPaths,
            CancellationToken cancellationToken
        )
        {
            var requestedPaths = objectPaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToHashSet(StringComparer.Ordinal);

            if (requestedPaths.Count == 0)
            {
                return;
            }

            var scheduledPaths = await dbContext
                .StorageCleanupTasks()
                .Where(task => requestedPaths.Contains(task.ObjectPath))
                .Select(task => task.ObjectPath)
                .ToListAsync(cancellationToken);

            requestedPaths.ExceptWith(scheduledPaths);
            requestedPaths.ExceptWith(
                dbContext.StorageCleanupTasks().Local.Select(task => task.ObjectPath)
            );

            foreach (var objectPath in requestedPaths)
            {
                dbContext.StorageCleanupTasks().Add(StorageCleanupTask.Create(objectPath));
            }
        }

        private async Task DeleteUnscheduledUploadsAsync(IReadOnlyCollection<string> objectPaths)
        {
            var deletedPaths = new HashSet<string>(StringComparer.Ordinal);

            foreach (var objectPath in objectPaths)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(objectPath, CancellationToken.None);
                    deletedPaths.Add(objectPath);
                }
                catch (Exception exception)
                {
                    _logger.LogCritical(
                        exception,
                        "Failed to delete unscheduled storage object {ObjectPath}",
                        objectPath
                    );
                }
            }

            if (deletedPaths.Count == 0)
            {
                return;
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                    CancellationToken.None
                );
                var mediaAssets = await dbContext
                    .MediaAssets()
                    .Where(asset => deletedPaths.Contains(asset.Path))
                    .ToListAsync(CancellationToken.None);

                dbContext.MediaAssets().RemoveRange(mediaAssets);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(
                    exception,
                    "Deleted storage objects but failed to remove matching media metadata for {ObjectPaths}",
                    deletedPaths
                );
            }
        }
    }
}
