using SoraSora.Modules.Media.Contracts.Upload;

namespace SoraSora.Modules.Media.Domain.Upload
{
    public record MediaUpload(ImageUploadJob Job, string Path, string ContentType);
}
