using Serilog;
using SoraSora.Api.Exceptions;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Media.Application.Extensions;
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
builder.Services.AddMediaModule();
builder.Services.AddInventoryModule();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

/* Temporarily disabled for manual endpoint testing.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
*/

// Bypass authorization even if WebApplication automatically adds auth middleware.
app.MapControllers().AllowAnonymous();

app.Run();
