using SoraSora.Api.Application.Controllers;
using SoraSora.Api.Application.Workflow;

namespace SoraSora.Api.Application.Extensions
{
    public static class ApiModuleServiceCollectionExtension
    {
        public static IServiceCollection AddApiModule(this IServiceCollection services)
        {
            services.AddScoped<OrderWorkflow>();
            services.AddScoped<CreateProductWorkflow>();

            services.AddControllers().AddApplicationPart(typeof(CreateProductController).Assembly);

            return services;
        }
    }
}
