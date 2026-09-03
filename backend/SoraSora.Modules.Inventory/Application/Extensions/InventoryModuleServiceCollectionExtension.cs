using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Infrastructure.Persistence;

public static class InventoryModuleServiceCollectionExtension
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddSingleton<IModelBuilderContributor, InventoryModuleModelBuilderContributor>();

        // services.AddValidatorsFromAssemblyContaining<>();
        services.AddScoped<IInventoryService, InventoryService>();

        // services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);

        return services;
    }
}
