using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Media.Application
{
    public interface IImageMediaService
    {
        Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyList<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        );
    }
}
