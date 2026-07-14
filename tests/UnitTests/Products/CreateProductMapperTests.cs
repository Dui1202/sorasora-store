using Microsoft.AspNetCore.Http;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Mappers;

namespace UnitTests.Products;

public sealed class CreateProductMapperTests
{
    [Fact]
    public void MapsBaseSkuToImplicitVariant()
    {
        var request = CreateRequest(hasVariation: false);

        var command = CreateProductDtoMapper.ToCreateProductCommand(request);

        Assert.Equal("BASE-SKU", Assert.Single(command.ProductVariants).Sku);
    }

    [Fact]
    public void MapsExplicitVariantSku()
    {
        var request = CreateRequest(hasVariation: true) with
        {
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            ProductVariants =
            [
                new CreateProductVariantRequest
                {
                    Name = "Strawberry",
                    Sku = "STRAWBERRY-SKU",
                    CostPrice = 100,
                    SellingPrice = 150,
                    Position = 0,
                    ProductVariantImage = new CreateProductVariantImageRequest
                    {
                        File = CreateFile(),
                    },
                },
            ],
        };

        var command = CreateProductDtoMapper.ToCreateProductCommand(request);

        Assert.Equal("STRAWBERRY-SKU", Assert.Single(command.ProductVariants).Sku);
    }

    private static CreateProductRequest CreateRequest(bool hasVariation)
    {
        return new CreateProductRequest
        {
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            HasVariation = hasVariation,
            BaseSku = "BASE-SKU",
            BaseCostPrice = 100,
            BaseSellingPrice = 150,
            ProductVariants = [],
            ProductImages =
            [
                new CreateProductImageRequest
                {
                    File = CreateFile(),
                    Position = 0,
                },
            ],
        };
    }

    private static IFormFile CreateFile()
    {
        var stream = new MemoryStream([1]);
        return new FormFile(stream, 0, stream.Length, "file", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };
    }
}
