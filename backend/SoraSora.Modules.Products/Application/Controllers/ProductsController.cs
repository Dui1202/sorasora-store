using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Products.Application.Controllers;

[ApiController]
[Route(ApiRoute.ProductsApiRoute)]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

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
}
