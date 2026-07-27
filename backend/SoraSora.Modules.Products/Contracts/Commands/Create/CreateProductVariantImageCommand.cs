using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductVariantImageCommand(IFormFile File, string? Alt);
