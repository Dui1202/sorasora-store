namespace SoraSora.Modules.Products.Contracts.Requests.Update;

public sealed record RetainedProductImageRequest(string Path, string? Alt, int Position);
