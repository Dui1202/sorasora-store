using SoraSora.Modules.Products.Domain;

namespace SoraSora.Modules.Products.Contracts.Responses;

public sealed record ProductResponse
{
    public required string Id { get; init; }
    public required Guid Version { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsPublished { get; init; }
    public SupplierCountry SupplierCountry { get; init; }
    public SellMode SellMode { get; init; }
    public bool IsArchived { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public IReadOnlyCollection<ProductImageDto> ProductImages { get; init; } = [];
    public IReadOnlyCollection<ProductVariantResponse> ProductVariants { get; init; } = [];
    public IReadOnlyCollection<VariationResponse> Variations { get; init; } = [];
}
