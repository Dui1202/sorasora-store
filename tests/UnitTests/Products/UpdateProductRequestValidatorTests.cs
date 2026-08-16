using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;

namespace UnitTests.Products;

public sealed class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = CreateValidator();

    [Fact]
    public async Task AcceptsExactlyOneVariantWhenVariationsAreEmpty()
    {
        var result = await _validator.ValidateAsync(CreateBaseRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RejectsMissingVariantWhenVariationsAreEmpty()
    {
        var request = CreateBaseRequest() with { RetainedProductVariants = [] };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AcceptsCompleteSelectionsWhenVariationsArePresent()
    {
        var request = CreateVariationRequest();

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RejectsDuplicateVariantOptionCombinations()
    {
        var request = CreateVariationRequest();
        var firstVariant = Assert.Single(request.RetainedProductVariants);
        request = request with
        {
            RetainedProductVariants =
            [
                firstVariant,
                firstVariant with { Id = "variant-2", Position = 1 },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Product variant option combinations must be unique."
        );
    }

    [Fact]
    public async Task RejectsRetainedOptionIdsRepeatedAcrossVariations()
    {
        var request = CreateVariationRequest();
        request = request with
        {
            Variations =
            [
                request.Variations.Single(),
                new UpdateVariationRequest
                {
                    Id = "variation-2",
                    ClientId = "size",
                    Name = "Size",
                    Position = 1,
                    VariationOptions =
                    [
                        new UpdateVariationOptionRequest
                        {
                            Id = "option-1",
                            ClientId = "large",
                            Value = "Large",
                            Position = 0,
                        },
                    ],
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage == "Variation option ids must be unique across the product."
        );
    }

    [Fact]
    public async Task RejectsNullNestedOptionCollectionsWithoutThrowing()
    {
        var request = CreateVariationRequest();
        request = request with
        {
            Variations =
            [
                request.Variations.Single() with { VariationOptions = null! },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }

    private static UpdateProductRequest CreateBaseRequest()
    {
        return new UpdateProductRequest
        {
            Id = "product-1",
            Version = Guid.NewGuid(),
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            RetainedImages =
            [
                new RetainedProductImageRequest(
                    MediaAssetId: "asset-1",
                    Alt: null,
                    Position: 0
                ),
            ],
            RetainedProductVariants =
            [
                new RetainedProductVariantRequest
                {
                    Id = "variant-1",
                    Name = "Pocky",
                    CostPrice = 100,
                    SellingPrice = 150,
                    Position = 0,
                },
            ],
        };
    }

    private static UpdateProductRequest CreateVariationRequest()
    {
        return CreateBaseRequest() with
        {
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
                    ],
                },
            ],
            RetainedProductVariants =
            [
                new RetainedProductVariantRequest
                {
                    Id = "variant-1",
                    Name = "Strawberry",
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
    }

    private static UpdateProductRequestValidator CreateValidator()
    {
        var variantImageValidator = new CreateProductVariantImageRequestValidator();
        var variantOptionValidator = new CreateProductVariantOptionRequestValidator();
        return new UpdateProductRequestValidator(
            new CreateProductImageRequestValidator(),
            new ExistingProductImageRequestValidator(),
            new CreateProductVariantRequestValidator(
                variantImageValidator,
                variantOptionValidator
            ),
            new ExistingProductVariantRequestValidator(
                variantImageValidator,
                variantOptionValidator
            ),
            new UpdateVariationRequestValidator(new UpdateVariationOptionRequestValidator())
        );
    }
}
