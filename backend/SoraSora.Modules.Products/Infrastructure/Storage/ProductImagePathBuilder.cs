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

        public static string BuildProductVariantImagePath(
            string productId,
            string productVariantId,
            string fileName
        )
        {
            var extension = Path.GetExtension(fileName);
            var imageName = Guid.NewGuid();

            return $"products/{productId}/{productVariantId}/images/{imageName}{extension}";
        }
    }
}
