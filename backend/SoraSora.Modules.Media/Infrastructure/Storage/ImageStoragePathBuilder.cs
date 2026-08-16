using SoraSora.Modules.Media.Contracts.Upload;
using SoraSora.Modules.Media.Domain;

namespace SoraSora.Modules.Media.Infrastructure.Storage;

internal static class ImageStoragePathBuilder
{
    public static string Build(ImageUploadJob job, ImageFormat format)
    {
        var extension = format switch
        {
            ImageFormat.Jpeg => ".jpg",
            ImageFormat.Png => ".png",
            ImageFormat.WebP => ".webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

        var fileName = $"{job.Id}{extension}";

        return job.Target switch
        {
            MediaStorageTarget.ProductImage target =>
                $"products/{target.ProductId}/images/{fileName}",
            MediaStorageTarget.ProductVariantImage target =>
                $"products/{target.ProductId}/{target.VariantId}/images/{fileName}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(ImageUploadJob.Target),
                job.Target,
                "Unsupported media storage target."
            ),
        };
    }
}
