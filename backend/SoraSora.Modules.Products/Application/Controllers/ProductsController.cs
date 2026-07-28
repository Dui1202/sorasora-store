using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Mappers;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Products.Application.Controllers;

[ApiController]
[Route(ApiRoute.ProductsApiRoute)]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductRequest request,
        [FromServices] IValidator<CreateProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult);
        }
        var command = CreateProductDtoMapper.ToCreateProductCommand(request);
        var product = await _productService.CreateProductAsync(command, cancellationToken);

        return Created($"{ApiRoute.ProductsApiRoute}/{product.Id}", product);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<ProductResponse>> Update(
        string id,
        [FromForm] UpdateProductRequest request,
        [FromServices] IValidator<UpdateProductRequest> validator,
        CancellationToken cancellationToken
    )
    {
        request = request with { Id = id };

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return this.ValidationProblem(validationResult);
        }

        var command = UpdateProductMapper.ToUpdateProductCommand(request);
        var product = await _productService.UpdateProductAsync(command, cancellationToken);

        return Ok(product);
    }

    public Task<IActionResult> Delete(string id)
    {
        throw new NotImplementedException();
    }
}
