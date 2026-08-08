using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Media.Domain
{
    public sealed record ImageUploadJob(IFormFile File, Func<string, string> BuildPath);
}
