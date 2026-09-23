using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Media.Contracts.Upload;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Media.Infrastructure;
using SoraSora.Modules.Media.Infrastructure.Storage;

namespace SoraSora.Modules.Media.Application
{
    /// <summary>
    /// Coordinates image processing, object storage, media metadata, and deferred cleanup.
    /// </summary>
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

        /// <summary>
        /// Processes and uploads a batch of images, then persists a media asset for each upload.
        /// </summary>
        /// <param name="jobs">The image files and storage targets to process.</param>
        /// <param name="cancellationToken">Cancels processing, uploading, or persistence.</param>
        /// <returns>
        /// The job-to-asset mappings for the uploaded images. Result order is not guaranteed.
        /// </returns>
        /// <remarks>
        /// If the pipeline fails, uploaded objects are deleted immediately when possible. Any
        /// remaining objects are queued for deferred cleanup when possible before the original
        /// failure is rethrown.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// <paramref name="cancellationToken"/> has been cancelled.
        /// </exception>
        public async Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyCollection<ImageUploadJob> jobs,
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
                    var mediaAsset = MediaAsset.Create(
                        upload.Path,
                        upload.ContentType,
                        upload.Job.Alt
                    );
                    uploadResults.Add(new MediaUploadResult(upload.Job.Id, mediaAsset.Id));

                    dbContext.MediaAssets().Add(mediaAsset);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                return uploadResults;
            }
            catch
            {
                await CleanUpFailedUploadsAsync(attemptedUploadPaths.Keys.ToArray());

                // Prefer the request cancellation when it caused the pipeline failure.
                cancellationToken.ThrowIfCancellationRequested();

                if (pipelineFailures.TryPeek(out var pipelineFailure))
                {
                    ExceptionDispatchInfo.Capture(pipelineFailure).Throw();
                }

                throw;
            }
        }

        /// <summary>
        /// Adds deferred cleanup tasks for the requested media assets that currently exist.
        /// </summary>
        /// <param name="mediaAssetIds">
        /// The media asset identifiers to schedule. Blank and duplicate values are ignored.
        /// </param>
        /// <param name="cancellationToken">Cancels the metadata lookup or cleanup scheduling.</param>
        /// <returns>A task that completes when the cleanup tasks have been added to the context.</returns>
        /// <remarks>
        /// This method does not save changes. The caller must save the injected database context
        /// so cleanup scheduling participates in the caller's transaction. Missing assets are
        /// logged and skipped.
        /// </remarks>
        /// <exception cref="OperationCanceledException">
        /// <paramref name="cancellationToken"/> has been cancelled.
        /// </exception>
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

        public async Task ScheduleFailedUploadCleanupAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        )
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                cancellationToken
            );

            var requestedAssetIds = mediaAssetIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.Ordinal);

            if (requestedAssetIds.Count == 0)
            {
                return;
            }

            var mediaAssetPathsById = await dbContext
                .MediaAssets()
                .Where(asset => requestedAssetIds.Contains(asset.Id))
                .ToDictionaryAsync(asset => asset.Id, asset => asset.Path, cancellationToken);

            // same missing-ID logging if desired

            await AddCleanupTasksAsync(dbContext, mediaAssetPathsById.Values, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates alt text for existing media assets tracked by the caller's database context.
        /// </summary>
        /// <remarks>This method does not save changes.</remarks>
        public async Task UpdateImageAltTextsAsync(
            IReadOnlyDictionary<string, string?> altTextByMediaAssetId,
            CancellationToken cancellationToken
        )
        {
            if (altTextByMediaAssetId.Count == 0)
            {
                return;
            }

            var mediaAssetIds = altTextByMediaAssetId.Keys.ToHashSet(StringComparer.Ordinal);
            var mediaAssetsById = await _dbContext
                .MediaAssets()
                .Where(asset => mediaAssetIds.Contains(asset.Id))
                .ToDictionaryAsync(asset => asset.Id, StringComparer.Ordinal, cancellationToken);

            if (mediaAssetsById.Count != mediaAssetIds.Count)
            {
                throw new InvalidOperationException(
                    "Cannot update image alt text because media metadata is missing."
                );
            }

            foreach (var (mediaAssetId, alt) in altTextByMediaAssetId)
            {
                mediaAssetsById[mediaAssetId].UpdateAlt(alt);
            }
        }

        /// <summary>
        /// Compensates for a failed upload pipeline by deleting uploaded objects, removing any
        /// matching metadata, and scheduling deferred cleanup for objects that remain.
        /// </summary>
        /// <param name="attemptedUploadPaths">The storage paths touched by the failed pipeline.</param>
        /// <returns>A task that completes after all available cleanup strategies have run.</returns>
        /// <remarks>
        /// Cleanup is deliberately independent of request cancellation. Failures are logged and
        /// suppressed so the original upload failure can be rethrown.
        /// </remarks>
        private async Task CleanUpFailedUploadsAsync(
            IReadOnlyCollection<string> attemptedUploadPaths
        )
        {
            if (attemptedUploadPaths.Count == 0)
            {
                return;
            }

            var pathsNeedingScheduledCleanup = new HashSet<string>(StringComparer.Ordinal);

            foreach (var objectPath in attemptedUploadPaths)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(objectPath, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    pathsNeedingScheduledCleanup.Add(objectPath);
                    _logger.LogWarning(
                        exception,
                        "Failed to delete uploaded storage object {ObjectPath}; scheduling cleanup",
                        objectPath
                    );
                }
            }

            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                    CancellationToken.None
                );
                var mediaAssets = await dbContext
                    .MediaAssets()
                    .Where(asset => attemptedUploadPaths.Contains(asset.Path))
                    .ToListAsync(CancellationToken.None);

                dbContext.MediaAssets().RemoveRange(mediaAssets);
                await AddCleanupTasksAsync(
                    dbContext,
                    pathsNeedingScheduledCleanup,
                    CancellationToken.None
                );
                await dbContext.SaveChangesAsync(CancellationToken.None);

                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to remove media metadata or schedule remaining cleanup for uploaded objects {ObjectPaths}",
                    attemptedUploadPaths
                );
            }

            await ScheduleCleanupWithNewContextAsync(attemptedUploadPaths);
        }

        /// <summary>
        /// Processes upload jobs sequentially and writes the resulting images to the bounded
        /// upload channel.
        /// </summary>
        /// <param name="jobs">The image upload jobs to process.</param>
        /// <param name="writer">The channel used to hand processed images to upload workers.</param>
        /// <param name="bufferedImageSlots">
        /// Limits the number of processed images retained in memory.
        /// </param>
        /// <param name="failures">Collects the failure that stops the pipeline.</param>
        /// <param name="pipelineCancellation">Cancels the remaining pipeline on failure.</param>
        /// <param name="cancellationToken">Cancels image processing and channel writes.</param>
        /// <returns>A task that completes after all jobs are produced or the pipeline stops.</returns>
        /// <remarks>The writer is always completed before the method exits.</remarks>
        private async Task ProduceProcessedImagesAsync(
            IReadOnlyCollection<ImageUploadJob> jobs,
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

                    // The producer owns the slot until the channel accepts the item.
                    var handedToConsumer = false;

                    try
                    {
                        await using var source = job.File.OpenReadStream();

                        var processedImage = _imageProcessor.Process(
                            source,
                            job.File.Length,
                            cancellationToken
                        );

                        var path = ImageStoragePathBuilder.Build(job, processedImage.Format);

                        await writer.WriteAsync(
                            new ProcessedImageUpload(job, processedImage, path),
                            cancellationToken
                        );

                        // Once queued, the consumer is responsible for releasing the slot.
                        handedToConsumer = true;
                    }
                    finally
                    {
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
                writer.TryComplete();
            }
        }

        /// <summary>
        /// Reads processed images from the channel and uploads them to object storage.
        /// </summary>
        /// <param name="reader">The channel containing processed images ready for upload.</param>
        /// <param name="bufferedImageSlots">
        /// Tracks processed-image capacity released after each consumed item.
        /// </param>
        /// <param name="attemptedUploadPaths">
        /// Records storage paths touched by the pipeline and rejects duplicate paths.
        /// </param>
        /// <param name="successfulUploads">Collects uploads ready for media asset persistence.</param>
        /// <param name="failures">Collects the failure that stops the pipeline.</param>
        /// <param name="pipelineCancellation">Cancels the remaining pipeline on failure.</param>
        /// <param name="cancellationToken">Cancels channel reads and storage uploads.</param>
        /// <returns>A task that completes when the channel closes or the pipeline stops.</returns>
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

        /// <summary>
        /// Adds cleanup tasks for storage paths that are not already persisted or tracked locally.
        /// </summary>
        /// <param name="dbContext">The context that queries and tracks cleanup tasks.</param>
        /// <param name="objectPaths">
        /// The storage paths to schedule. Blank and duplicate values are ignored.
        /// </param>
        /// <param name="cancellationToken">Cancels the existing-task lookup.</param>
        /// <returns>A task that completes when missing cleanup tasks are tracked.</returns>
        /// <remarks>This method does not save changes.</remarks>
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

        /// <summary>
        /// Uses a fresh database context to persist cleanup tasks when the primary cleanup path
        /// cannot complete.
        /// </summary>
        /// <param name="objectPaths">The storage paths that still require cleanup.</param>
        /// <returns>A task that completes after the fallback scheduling attempt.</returns>
        /// <remarks>
        /// This last-resort operation ignores request cancellation and logs rather than rethrows
        /// failures so it cannot replace the original upload error.
        /// </remarks>
        private async Task ScheduleCleanupWithNewContextAsync(
            IReadOnlyCollection<string> objectPaths
        )
        {
            try
            {
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                    CancellationToken.None
                );

                await AddCleanupTasksAsync(dbContext, objectPaths, CancellationToken.None);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(
                    exception,
                    "Failed to schedule cleanup for uploaded objects {ObjectPaths}",
                    objectPaths
                );
            }
        }
    }
}
