namespace SoraSora.Modules.Products.Contracts.Requests.Update;

public sealed record RetainedProductImageRequest(
    string MediaAssetId,
    string? Alt,
    int Position
);
