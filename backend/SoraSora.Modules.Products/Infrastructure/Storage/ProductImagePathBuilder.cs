namespace SoraSora.Modules.Products.Infrastructure.Storage
{
    public static class ProductImagePathBuilder
    {
        public static string BuildProductImagePath(string productId, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var imageName = Guid.NewGuid();

            return $"products/{productId}/images/{imageName}{extension}";
        }
    }
}
