using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Products.Application.Controllers;
using SoraSora.Modules.Products.Application.Images;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Infrastructure.Images;
using SoraSora.Modules.Products.Infrastructure.Persistence;

namespace SoraSora.Modules.Products.Application.Extensions;

public static class ProductsModuleServiceCollectionExtension
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services)
    {
        services.AddSingleton<IModelBuilderContributor, ProductsModuleModelBuilderContributor>();

        services.AddSingleton<IProductImageProcessor, SkiaProductImageProcessor>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddScoped<IProductService, ProductService>();

        services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);

        return services;
    }
}
