public interface IProductVariantLookupService
{
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);
}
