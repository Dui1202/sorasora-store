using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands
{
    public sealed record CreateProductImageCommand(IFormFile File, string? Alt, int Position);
}
