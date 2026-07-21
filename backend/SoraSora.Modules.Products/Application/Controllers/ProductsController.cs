using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Mappers;
using SoraSora.Shared.Extension;
using SoraSora.Shared.Route;

namespace SoraSora.Modules.Products.Application.Controllers
{
    [ApiController]
    [Route(ApiRoute.ProductApiRoute)]
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

            return Created($"{ApiRoute.ProductApiRoute}/{product.Id}", product);
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
    }
}
