namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record RetainedProductImageCommand(string MediaAssetId, string? Alt, int Position);
