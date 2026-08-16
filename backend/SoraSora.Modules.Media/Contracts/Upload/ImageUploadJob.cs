using Microsoft.AspNetCore.Http;
using NUlid;

namespace SoraSora.Modules.Media.Contracts.Upload;

public sealed record ImageUploadJob
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public IFormFile File { get; }
    public MediaStorageTarget Target { get; }
    public string? Alt { get; }

    public ImageUploadJob(IFormFile file, MediaStorageTarget target, string? alt = null)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(target);

        File = file;
        Target = target;
        Alt = alt;
    }
}
