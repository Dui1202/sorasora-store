namespace SoraSora.Modules.Media.Domain
{
    public record ProcessedImageUpload(
        ImageUploadJob ImageUploadJob,
        ProcessedImage Image,
        string Path
    );
}
