namespace SoraSora.Modules.Products.Contracts.Commands.Create
{
    public sealed record CreateVariationOptionCommand
    {
        public required string Value { get; init; }
        public int Position { get; init; }
    }
}
