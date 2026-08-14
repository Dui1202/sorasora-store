using SoraSora.Modules.Media.Domain.Upload;

namespace SoraSora.Modules.Media.Application
{
    public interface IImageMediaService
    {
        Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyList<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        );

        Task DeleteImagesAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        );

        Task ScheduleImageDeletionAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        );
    }
}
