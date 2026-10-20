using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Orders.Application.Services;
using SoraSora.Modules.Orders.Infrastructure.Persistence;

namespace SoraSora.Modules.Orders.Application.Extensions;

public static class OrdersModuleServiceCollectionExtension
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddSingleton<IModelBuilderContributor, OrdersModuleModelBuilderContributor>();

        // services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
        services.AddScoped<IOrderService, OrderService>();

        // services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);

        return services;
    }
}
