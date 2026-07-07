using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Requests.Create
{
    public sealed record CreateProductImageRequest
    {
        public IFormFile Image { get; init; } = null!;
        public required string Path { get; init; }
        public string? Alt { get; init; }
        public required int Position { get; init; }
    }
}
