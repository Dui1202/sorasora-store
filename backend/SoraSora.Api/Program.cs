using SoraSora.Infrastructure.Persistence.Extensions;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Infrastructure.Storage;
using SoraSora.Modules.Products.Application;
using SoraSora.Modules.Products.Application.Controllers;
using SoraSora.Modules.Products.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IModelBuilderContributor, ProductModeBuilderContributor>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSupabaseStorage(builder.Configuration);

builder.Services.AddControllers().AddApplicationPart(typeof(ProductsController).Assembly);

var app = builder.Build();

app.MapControllers();

app.Run();
