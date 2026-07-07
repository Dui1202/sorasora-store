using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Abstractions;

namespace SoraSora.Infrastructure.Storage;

public static class SupabaseStorageDependencyInjection
{
    public static IServiceCollection AddSupabaseStorage(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<SupabaseStorageOptions>(configuration.GetSection("SupabaseStorage"));

        var options =
            configuration.GetSection("SupabaseStorage").Get<SupabaseStorageOptions>()
            ?? throw new InvalidOperationException("SupabaseStorage config is missing.");

        if (string.IsNullOrWhiteSpace(options.AccessKeyId))
            throw new InvalidOperationException("SupabaseStorage:AccessKeyId is missing.");

        if (string.IsNullOrWhiteSpace(options.SecretAccessKey))
            throw new InvalidOperationException("SupabaseStorage:SecretAccessKey is missing.");

        var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);

        var s3Config = new AmazonS3Config
        {
            ServiceURL = options.Endpoint,
            AuthenticationRegion = options.Region,
            ForcePathStyle = true,
        };

        services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));

        services.AddScoped<IFileStorageService, SupabaseFileStorageService>();

        return services;
    }
}
