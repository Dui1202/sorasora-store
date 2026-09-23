using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductVariantImageCommand(string? Alt);
