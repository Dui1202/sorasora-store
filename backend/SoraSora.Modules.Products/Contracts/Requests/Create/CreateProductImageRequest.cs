using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Requests.Create;

public sealed record CreateProductImageRequest
{
    public required IFormFile File { get; init; }
    public string? Alt { get; init; }
    public required int Position { get; init; }
}
