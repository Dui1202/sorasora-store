namespace SoraSora.Modules.Products.Application.Images;

public interface IProductImageProcessor
{
    ProcessedProductImage Process(
        Stream source,
        long sourceLength,
        CancellationToken cancellationToken
    );
}
