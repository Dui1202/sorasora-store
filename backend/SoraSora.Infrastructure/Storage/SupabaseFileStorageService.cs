using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SoraSora.Infrastructure.Abstractions;

namespace SoraSora.Infrastructure.Storage;

public class SupabaseFileStorageService(
    IAmazonS3 s3Client,
    IOptions<SupabaseStorageOptions> options
) : IFileStorageService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly SupabaseStorageOptions _options = options.Value;

    public async Task<string> UploadFileAsync(
        Stream content,
        string path,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = path,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false,
        };

        try
        {
            await _s3Client.PutObjectAsync(request, cancellationToken);
        }
        catch (AmazonS3Exception exception)
        {
            throw new FileStorageException("upload", path, exception);
        }

        return path;
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken)
    {
        var request = new DeleteObjectRequest { BucketName = _options.Bucket, Key = path };

        try
        {
            await _s3Client.DeleteObjectAsync(request, cancellationToken);
        }
        catch (AmazonS3Exception exception)
        {
            throw new FileStorageException("deletion", path, exception);
        }
    }
}
