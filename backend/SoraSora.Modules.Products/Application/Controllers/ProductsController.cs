using Microsoft.AspNetCore.Mvc;
using SoraSora.Modules.Products.Contracts.Requests.Create;

namespace SoraSora.Modules.Products.Application.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);

            return Created($"/api/products/{product.Id}", product);
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
