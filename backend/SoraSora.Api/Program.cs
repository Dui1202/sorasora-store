using FluentValidation;
using Serilog;
using SoraSora.Api.Exceptions;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Infrastructure.Storage;
using SoraSora.Modules.Products.Application;
using SoraSora.Modules.Products.Application.Controllers;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    }
);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IModelBuilderContributor, ProductModeBuilderContributor>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSupabaseStorage(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();
