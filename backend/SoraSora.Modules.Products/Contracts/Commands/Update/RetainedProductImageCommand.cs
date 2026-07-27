namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record RetainedProductImageCommand(string Path, string? Alt, int Position);
