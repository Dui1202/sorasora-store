using SoraSora.Modules.Media.Contracts.Upload;
using SoraSora.Modules.Media.Domain.Upload;

namespace SoraSora.Modules.Media.Application
{
    public interface IImageMediaService
    {
        Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyCollection<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        );

        Task ScheduleImageDeletionAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        );

        Task UpdateImageAltTextsAsync(
            IReadOnlyDictionary<string, string?> altTextByMediaAssetId,
            CancellationToken cancellationToken
        );
    }
}
