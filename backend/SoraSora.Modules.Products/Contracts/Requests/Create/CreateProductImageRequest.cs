namespace SoraSora.Modules.Products.Contracts.Requests.Create
{
    public sealed record CreateProductImageRequest
    {
        public required string Path { get; init; }
    }
}
