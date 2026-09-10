namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record ProductVariantResponse
{
    public required string Id { get; init; }
    public required string ProductId { get; init; }
    public required string Name { get; init; }
    public string? Sku { get; init; }
    public int CostPrice { get; init; }
    public int SellingPrice { get; init; }
    public int Position { get; init; }
    public bool IsArchived { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
