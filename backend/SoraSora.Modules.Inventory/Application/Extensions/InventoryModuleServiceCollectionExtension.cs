using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Inventory.Application.Controllers;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Inventory.Contracts.Validators;
using SoraSora.Modules.Inventory.Infrastructure.Persistence.Seeding;
using SoraSora.Modules.Inventory.Infrastructure.Persistence;

namespace SoraSora.Modules.Inventory.Application.Extensions;

public static class InventoryModuleServiceCollectionExtension
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddSingleton<IModelBuilderContributor, InventoryModuleModelBuilderContributor>();
        services.AddScoped<IDbSeeder, WarehouseSeeder>();

        services.AddValidatorsFromAssemblyContaining<CreateWarehouseRequestValidator>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IWarehouseService, WarehouseService>();

        services.AddControllers().AddApplicationPart(typeof(WarehousesController).Assembly);

        return services;
    }
}
