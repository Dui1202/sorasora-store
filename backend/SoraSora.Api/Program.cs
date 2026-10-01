using Serilog;
using SoraSora.Api.Application.Extensions;
using SoraSora.Api.Exceptions;
using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Auth.Application.Extensions;
using SoraSora.Modules.Inventory.Application.Extensions;
using SoraSora.Modules.Media.Application.Extensions;
using SoraSora.Modules.Orders.Application.Extensions;
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

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
        }
    );
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProductsModule();
builder.Services.AddOrdersModule();
builder.Services.AddAuthModule();
builder.Services.AddMediaModule();
builder.Services.AddInventoryModule();
builder.Services.AddApiModule();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseCors(FrontendCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

/* Temporarily disabled for manual endpoint testing.

app.MapControllers();
*/

// Bypass authorization even if WebApplication automatically adds auth middleware.
app.MapControllers().AllowAnonymous();

app.Run();
