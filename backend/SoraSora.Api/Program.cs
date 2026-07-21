using Serilog;
using SoraSora.Api.Exceptions;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Products.Application.Extensions;

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
builder.Services.AddAuthModule();
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
