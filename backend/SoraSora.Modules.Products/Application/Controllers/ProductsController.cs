using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts;
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

    [HasPermission(AppPermissions.Products.Create)]
    [RequestSizeLimit(ProductUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProductUploadLimits.MaxRequestBytes)]
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
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }
        var command = CreateProductDtoMapper.ToCreateProductCommand(request);
        var product = await _productService.CreateProductAsync(command, cancellationToken);

        return Created($"{ApiRoute.ProductsApiRoute}/{product.Id}", product);
    }

    [HasPermission(AppPermissions.Products.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();

        return Ok(products);
    }

    [HasPermission(AppPermissions.Products.Read)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        return Ok(product);
    }

    [HasPermission(AppPermissions.Products.Update)]
    [RequestSizeLimit(ProductUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ProductUploadLimits.MaxRequestBytes)]
    [HttpPut("{id}")]
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
            return this.ValidationProblem(validationResult, ProductErrorCodes.ValidationFailed);
        }

        var command = UpdateProductMapper.ToUpdateProductCommand(request);
        var product = await _productService.UpdateProductAsync(command, cancellationToken);

        return Ok(product);
    }

    [HasPermission(AppPermissions.Products.Delete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        Guid version,
        CancellationToken cancellationToken
    )
    {
        await _productService.DeleteProductAsync(id, version, cancellationToken);

        return NoContent();
    }
}
