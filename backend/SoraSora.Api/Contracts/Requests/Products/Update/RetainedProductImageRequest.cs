namespace SoraSora.Api.Contracts.Requests.Products.Update;

public sealed record RetainedProductImageRequest(
    string MediaAssetId,
    string? Alt,
    int Position
);
