using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Modules.Products.Application;
using SoraSora.Modules.Products.Contracts.Requests.Create;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.MapPost(
    "/products",
    async (CreateProductRequest request, IProductService productService) =>
    {
        var product = await productService.CreateProductAsync(request);
        return Results.Created($"/products/{product.Id}", product);
    }
);

app.Run();
