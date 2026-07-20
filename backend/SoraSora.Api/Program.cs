using Serilog;
using SoraSora.Api.Exceptions;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Infrastructure.Storage;
using SoraSora.Modules.Products.Application;
using SoraSora.Modules.Products.Application.Controllers;
using SoraSora.Modules.Products.Application.Extensions;
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
builder.Services.AddProductsModule();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
