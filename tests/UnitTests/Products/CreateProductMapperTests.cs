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
        var request = CreateRequest();

        var command = CreateProductDtoMapper.ToCreateProductCommand(request);

        Assert.Equal("BASE-SKU", Assert.Single(command.ProductVariants).Sku);
    }

    [Fact]
    public void MapsExplicitVariantSku()
    {
        var request = CreateRequest() with
        {
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            Variations =
            [
                new CreateVariationRequest
                {
                    ClientId = "flavor",
                    Name = "Flavor",
                    Position = 0,
                    VariationOptions =
                    [
                        new CreateVariationOptionRequest
                        {
                            ClientId = "strawberry",
                            Value = "Strawberry",
                            Position = 0,
                        },
                    ],
                },
            ],
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

    private static CreateProductRequest CreateRequest()
    {
        return new CreateProductRequest
        {
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
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
