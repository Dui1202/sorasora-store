namespace SoraSora.Modules.Products.Domain;

public sealed class ProductDomainException(string message, string? field = null)
    : Exception(message)
{
    public string? Field { get; } = field;
}
