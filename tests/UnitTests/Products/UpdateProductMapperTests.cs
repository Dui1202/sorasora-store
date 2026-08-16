using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Mappers;

namespace UnitTests.Products;

public sealed class UpdateProductMapperTests
{
    [Fact]
    public void MapsDesiredVariationGraphAndRetainedVariantSelections()
    {
        var request = new UpdateProductRequest
        {
            Id = "product-1",
            Version = Guid.NewGuid(),
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            Variations =
            [
                new UpdateVariationRequest
                {
                    Id = "variation-1",
                    ClientId = "flavor",
                    Name = "Flavor",
                    Position = 0,
                    VariationOptions =
                    [
                        new UpdateVariationOptionRequest
                        {
                            Id = "option-1",
                            ClientId = "strawberry",
                            Value = "Strawberry",
                            Position = 0,
                        },
                        new UpdateVariationOptionRequest
                        {
                            ClientId = "chocolate",
                            Value = "Chocolate",
                            Position = 1,
                        },
                    ],
                },
            ],
            RetainedProductVariants =
            [
                new RetainedProductVariantRequest
                {
                    Id = "variant-1",
                    Name = "Strawberry",
                    Sku = "STRAWBERRY",
                    CostPrice = 100,
                    SellingPrice = 150,
                    Position = 0,
                    ProductVariantOptions =
                    [
                        new CreateProductVariantOptionRequest
                        {
                            VariationClientId = "flavor",
                            VariationOptionClientId = "strawberry",
                        },
                    ],
                },
            ],
        };

        var command = UpdateProductMapper.ToUpdateProductCommand(request);

        var variation = Assert.Single(command.Variations);
        Assert.Equal("variation-1", variation.Id);
        Assert.Equal("flavor", variation.ClientId);
        Assert.Collection(
            variation.VariationOptions,
            option =>
            {
                Assert.Equal("option-1", option.Id);
                Assert.Equal("strawberry", option.ClientId);
            },
            option =>
            {
                Assert.Null(option.Id);
                Assert.Equal("chocolate", option.ClientId);
            }
        );

        var selection = Assert.Single(
            Assert.Single(command.RetainedProductVariants).ProductVariantOptions
        );
        Assert.Equal("flavor", selection.VariationClientId);
        Assert.Equal("strawberry", selection.VariationOptionClientId);
    }
}
