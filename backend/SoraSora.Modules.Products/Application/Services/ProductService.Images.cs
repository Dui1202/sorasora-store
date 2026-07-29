using Microsoft.AspNetCore.Http;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Modules.Products.Application.Exceptions;
using SoraSora.Modules.Products.Application.Images;

namespace SoraSora.Modules.Products.Application.Services;

public sealed partial class ProductService
{
    private async Task<string> ProcessAndUploadImageAsync(
        IFormFile file,
        Func<ProductImageFormat, string> buildPath,
        ICollection<string> uploadedImagePaths,
        CancellationToken cancellationToken
    )
    {
        await using var source = file.OpenReadStream();
        var processedImage = _productImageProcessor.Process(
            source,
            file.Length,
            cancellationToken
        );
        var path = buildPath(processedImage.Format);

        uploadedImagePaths.Add(path);

        await using var content = new MemoryStream(processedImage.Content, writable: false);
        try
        {
            await _fileStorageService.UploadFileAsync(
                content,
                path,
                processedImage.ContentType,
                cancellationToken
            );
        }
        catch (FileStorageException exception)
        {
            throw new ProductStorageException(exception);
        }

        return path;
    }
}
