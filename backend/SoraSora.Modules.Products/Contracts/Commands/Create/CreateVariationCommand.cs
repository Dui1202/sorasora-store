namespace SoraSora.Modules.Products.Contracts.Commands.Create
{
    public sealed record CreateVariationCommand
    {
        public required string Name { get; init; }
        public int Position { get; init; }
        public required IReadOnlyCollection<CreateVariationOptionCommand> VariationOptions { get; init; }
    }
}
