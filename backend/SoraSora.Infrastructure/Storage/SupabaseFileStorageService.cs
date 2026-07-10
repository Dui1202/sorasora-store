using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SoraSora.Infrastructure.Abstractions;

namespace SoraSora.Infrastructure.Storage
{
    public class SupabaseFileStorageService(
        IAmazonS3 s3Client,
        IOptions<SupabaseStorageOptions> options
    ) : IFileStorageService
    {
        private const long MaxImageSize = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp",
        ];

        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly SupabaseStorageOptions _options = options.Value;

        public async Task<string> UploadFileAsync(
            IFormFile file,
            string path,
            CancellationToken cancellationToken = default
        )
        {
            ValidateImage(file);
            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _options.Bucket,
                Key = path,
                InputStream = stream,
                ContentType = file.ContentType,
                AutoCloseStream = false,
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return path;
        }

        public async Task DeleteFileAsync(string path, CancellationToken cancellationToken)
        {
            var request = new DeleteObjectRequest { BucketName = _options.Bucket, Key = path };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
        }

        private static void ValidateImage(IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new ArgumentException("Image file is empty.");
            }

            if (file.Length > MaxImageSize)
            {
                throw new ArgumentException("Image size must be smaller than 5MB.");
            }

            if (!AllowedImageContentTypes.Contains(file.ContentType))
            {
                throw new ArgumentException("Only jpeg, png and webq images are allowed.");
            }
        }
    }
}
