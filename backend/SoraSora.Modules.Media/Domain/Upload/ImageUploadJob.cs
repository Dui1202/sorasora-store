using Microsoft.AspNetCore.Http;
using NUlid;

namespace SoraSora.Modules.Media.Domain.Upload
{
    public sealed record ImageUploadJob(IFormFile File, Func<string, string> BuildPath)
    {
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
    };
}
