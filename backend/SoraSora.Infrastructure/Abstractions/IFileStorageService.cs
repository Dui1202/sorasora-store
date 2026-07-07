using Microsoft.AspNetCore.Http;

namespace SoraSora.Infrastructure.Abstractions
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(
            IFormFile file,
            string path,
            CancellationToken cancellationToken
        );
        Task DeleteFileAsync(string path, CancellationToken cancellationToken);
    }
}
