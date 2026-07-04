using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Products.Application;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IModelBuilderContributor, ProductModeBuilderContributor>();
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

app.MapGet(
    "/products",
    async (IProductService productService) =>
    {
        var products = await productService.GetAllProductsAsync();

        return Results.Ok(products);
    }
);

app.MapGet(
    "/products/{id}",
    async (string id, IProductService productService) =>
    {
        var product = await productService.GetProductByIdAsync(id);
        if (product is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(product);
    }
);

app.Run();
