using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Domain;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Media.Infrastructure;

namespace SoraSora.Modules.Media.Application
{
    public class ImageMediaService(
        IDbContextFactory<SoraSoraDbContext> dbContextFactory,
        IImageProcessor imageProcessor,
        IFileStorageService fileStorageService
    ) : IImageMediaService
    {
        private readonly IDbContextFactory<SoraSoraDbContext> _dbContextFactory = dbContextFactory;
        private readonly IImageProcessor _imageProcessor = imageProcessor;
        private readonly IFileStorageService _fileStorageService = fileStorageService;

        private const int UploadWorkerCount = 2;
        private const int MaxBufferedImages = 2;

        public async Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyList<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        )
        {
            ConcurrentDictionary<string, byte> attemptedUploadPaths = new(StringComparer.Ordinal);
            ConcurrentBag<MediaUploadResult> successfullyUploadedImages = [];

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
                        successfullyUploadedImages,
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

                foreach (var result in successfullyUploadedImages)
                {
                    var mediaAsset = MediaAsset.Create(result.Path, result.ContentType);

                    dbContext.MediaAssets().Add(mediaAsset);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                var failedCleanupPaths = new List<string>();

                foreach (var path in attemptedUploadPaths.Keys)
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(path, CancellationToken.None);
                    }
                    catch (Exception)
                    {
                        // TODO: log cleanupException
                        failedCleanupPaths.Add(path);
                    }
                }

                if (failedCleanupPaths.Count > 0)
                {
                    await using var dbContext = await _dbContextFactory.CreateDbContextAsync(
                        cancellationToken
                    );

                    try
                    {
                        foreach (var path in failedCleanupPaths)
                        {
                            dbContext.StorageCleanupTasks().Add(StorageCleanupTask.Create(path));
                        }

                        await dbContext.SaveChangesAsync(CancellationToken.None);
                    }
                    catch (Exception)
                    {
                        // TODO: log this as a serious cleanup/reconciliation failure.
                    }
                }
                //Priotize throwing the http CancellationToken first because it could be the reason why the pipeline failed
                cancellationToken.ThrowIfCancellationRequested();

                if (pipelineFailures.TryPeek(out var pipelineFailure))
                {
                    ExceptionDispatchInfo.Capture(pipelineFailure).Throw();
                }

                throw;
            }

            return successfullyUploadedImages.ToList();
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
            ConcurrentBag<MediaUploadResult> successfullyUploadedImages,
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

                        successfullyUploadedImages.Add(new MediaUploadResult(path, contentType));
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
    }
}
