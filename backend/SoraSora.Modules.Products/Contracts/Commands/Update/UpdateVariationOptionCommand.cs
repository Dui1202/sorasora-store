namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record UpdateVariationOptionCommand
{
    public string? Id { get; init; }
    public required string ClientId { get; init; }
    public required string Value { get; init; }
    public int Position { get; init; }
}
