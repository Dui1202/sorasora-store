using Microsoft.AspNetCore.Http;

namespace SoraSora.Modules.Products.Contracts.Commands.Create;

public sealed record CreateProductImageCommand(string ClientId, string? Alt, int Position);
