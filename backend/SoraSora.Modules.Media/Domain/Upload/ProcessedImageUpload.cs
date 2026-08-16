using SoraSora.Modules.Media.Contracts.Upload;

namespace SoraSora.Modules.Media.Domain.Upload
{
    public record ProcessedImageUpload(
        ImageUploadJob ImageUploadJob,
        ProcessedImage Image,
        string Path
    );
}
