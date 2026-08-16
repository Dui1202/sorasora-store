namespace SoraSora.Modules.Products.Contracts.Commands.Update;

public sealed record UpdateVariationCommand
{
    public string? Id { get; init; }
    public required string ClientId { get; init; }
    public required string Name { get; init; }
    public int Position { get; init; }
    public IReadOnlyCollection<UpdateVariationOptionCommand> VariationOptions { get; init; } = [];
}
