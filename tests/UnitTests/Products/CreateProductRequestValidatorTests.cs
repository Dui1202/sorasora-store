using Microsoft.AspNetCore.Http;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;

namespace UnitTests.Products;

public sealed class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = CreateValidator();

    [Fact]
    public async Task UsesBaseFieldsWhenVariationGraphIsEmpty()
    {
        var result = await _validator.ValidateAsync(CreateBaseRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UsesExplicitVariantsWhenVariationGraphIsPresent()
    {
        var request = CreateBaseRequest() with
        {
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
                    CostPrice = 100,
                    SellingPrice = 150,
                    Position = 0,
                    ProductVariantImage = new CreateProductVariantImageRequest
                    {
                        File = CreateFile(),
                    },
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

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    private static CreateProductRequest CreateBaseRequest() =>
        new()
        {
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            BaseCostPrice = 100,
            BaseSellingPrice = 150,
            ProductImages =
            [
                new CreateProductImageRequest { File = CreateFile(), Position = 0 },
            ],
        };

    private static CreateProductRequestValidator CreateValidator()
    {
        var variantOptionValidator = new CreateProductVariantOptionRequestValidator();
        return new CreateProductRequestValidator(
            new CreateProductImageRequestValidator(),
            new CreateProductVariantRequestValidator(
                new CreateProductVariantImageRequestValidator(),
                variantOptionValidator
            ),
            new CreateVariationRequestValidator(new CreateVariationOptionRequestValidator())
        );
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
