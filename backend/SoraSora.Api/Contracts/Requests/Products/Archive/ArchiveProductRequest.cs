public sealed record ArchiveProductRequest
{
    public required string ProductId { get; init; }
    public required Guid Version { get; init; }
}
