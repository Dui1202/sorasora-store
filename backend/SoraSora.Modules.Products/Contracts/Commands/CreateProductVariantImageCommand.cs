using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands
{
    public sealed record CreateProductVariantImageCommand(IFormFile File, string? Alt);
}
