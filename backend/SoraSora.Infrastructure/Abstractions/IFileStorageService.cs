namespace SoraSora.Infrastructure.Abstractions;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream content,
        string path,
        string contentType,
        CancellationToken cancellationToken
    );

    Task DeleteFileAsync(string path, CancellationToken cancellationToken);
}
