using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Requests.Create;

public sealed record CreateProductVariantImageRequest
{
    public required IFormFile File { get; init; }
    public string? Alt { get; init; }
}
