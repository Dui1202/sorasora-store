using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Abstractions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Media.Infrastructure;
using SoraSora.Modules.Media.Infrastructure.Persistence;
using SoraSora.Modules.Media.Infrastructure.Storage;

namespace SoraSora.Modules.Media.Application.Extensions
{
    public static class MediaModuleServiceCollectionExtension
    {
        public static IServiceCollection AddMediaModule(this IServiceCollection services)
        {
            services.AddSingleton<IModelBuilderContributor, MediaModuleModelBuilderContributor>();
            services.AddSingleton<IImageProcessor, SkiaImageProcessor>();

            services.AddScoped<IImageMediaService, ImageMediaService>();
            services.AddScoped<IStorageCleanupProcessor, MediaStorageCleanupProcessor>();

            return services;
        }
    }
}
