using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductImageCommand(IFormFile File, string? Alt, int Position);
