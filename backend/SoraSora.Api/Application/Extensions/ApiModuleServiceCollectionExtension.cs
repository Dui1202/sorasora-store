using FluentValidation;
using SoraSora.Api.Application.Controllers;
using SoraSora.Api.Contracts.Validators.Products;
using SoraSora.Api.Application.Workflow;
using SoraSora.Api.Application.Workflow.Products;

namespace SoraSora.Api.Application.Extensions
{
    public static class ApiModuleServiceCollectionExtension
    {
        public static IServiceCollection AddApiModule(this IServiceCollection services)
        {
            services.AddScoped<OrderWorkflow>();
            services.AddScoped<ProductWorkflow>();
            services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

            services
                .AddControllers()
                .AddApplicationPart(typeof(ProductWorkflowController).Assembly);

            return services;
        }
    }
}
