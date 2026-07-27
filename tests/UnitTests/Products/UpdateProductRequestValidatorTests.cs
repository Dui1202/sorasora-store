using Microsoft.AspNetCore.Http;
using SoraSora.Modules.Products.Contracts;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Requests.Update;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;

namespace UnitTests.Products;

public sealed class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = new(
        new CreateProductImageRequestValidator(),
        new ExistingProductImageRequestValidator(),
        new CreateProductVariantRequestValidator(new CreateProductVariantImageRequestValidator()),
        new ExistingProductVariantRequestValidator(
            new ExistingProductVariantImageRequestValidator(),
            new CreateProductVariantImageRequestValidator()
        )
    );

    [Fact]
    public async Task AcceptsValidProductWithoutVariations()
    {
        var result = await _validator.ValidateAsync(CreateValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AcceptsValidProductWithVariations()
    {
        var result = await _validator.ValidateAsync(CreateValidVariationRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task RejectsInvalidProductFields()
    {
        var request = CreateValidRequest() with
        {
            Id = "",
            Name = new string('a', Product.MaxNameLength + 1),
            Description = new string('a', Product.MaxDescriptionLength + 1),
            SupplierCountry = (SupplierCountry)int.MaxValue,
            SellMode = (SellMode)int.MaxValue,
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Id));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Description));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.SupplierCountry)
        );
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.SellMode));
    }

    [Fact]
    public async Task RejectsProductWithoutImages()
    {
        var request = CreateValidRequest() with { RetainedImages = [], NewImages = [] };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedImages)
        );
    }

    [Fact]
    public async Task RejectsTooManyImagesAcrossExistingAndNewCollections()
    {
        var request = CreateValidRequest() with
        {
            RetainedImages = Enumerable
                .Range(0, ProductUploadLimits.MaxProductImages)
                .Select(CreateExistingImage)
                .ToArray(),
            NewImages =
            [
                new CreateProductImageRequest
                {
                    File = CreateFile(),
                    Position = ProductUploadLimits.MaxProductImages,
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedImages)
        );
    }

    [Fact]
    public async Task RejectsDuplicateImagePositionsAcrossExistingAndNewCollections()
    {
        var request = CreateValidRequest() with
        {
            RetainedImages = [CreateExistingImage(position: 0)],
            NewImages = [new CreateProductImageRequest { File = CreateFile(), Position = 0 }],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedImages)
        );
    }

    [Fact]
    public async Task ValidatesExistingAndNewImages()
    {
        var request = CreateValidRequest() with
        {
            RetainedImages = [new RetainedProductImageRequest("", new string('a', 201), -1)],
            NewImages =
            [
                new CreateProductImageRequest { File = CreateFile(length: 0), Position = 1 },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Path"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Alt"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Position"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".File"));
    }

    [Fact]
    public async Task ProductWithoutVariationsRequiresBasePricingAndNoVariantCollections()
    {
        var request = CreateValidRequest() with
        {
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            RetainedProductVariants = [CreateExistingVariant()],
            NewProductVariants = [CreateNewVariant(position: 1, sku: "SKU-2")],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.BaseSku));
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.BaseCostPrice)
        );
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.BaseSellingPrice)
        );
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedProductVariants)
        );
        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.NewProductVariants)
        );
    }

    [Fact]
    public async Task ProductWithVariationsRequiresAtLeastOneVariant()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants = [],
            NewProductVariants = [],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedProductVariants)
        );
    }

    [Fact]
    public async Task RejectsTooManyVariantsAcrossExistingAndNewCollections()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants = Enumerable
                .Range(0, ProductUploadLimits.MaxProductVariants)
                .Select(position =>
                    CreateExistingVariant(position: position, sku: $"SKU-{position}")
                )
                .ToArray(),
            NewProductVariants =
            [
                CreateNewVariant(
                    position: ProductUploadLimits.MaxProductVariants,
                    sku: "OVER-LIMIT"
                ),
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.RetainedProductVariants)
        );
    }

    [Fact]
    public async Task RejectsDuplicateVariantPositionsAcrossCollections()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants = [CreateExistingVariant(position: 0, sku: "SKU-1")],
            NewProductVariants = [CreateNewVariant(position: 0, sku: "SKU-2")],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(request.RetainedProductVariants)
                && error.ErrorMessage.Contains("positions")
        );
    }

    [Fact]
    public async Task RejectsDuplicateVariantSkusAcrossCollectionsIgnoringCase()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants = [CreateExistingVariant(position: 0, sku: "SKU-1")],
            NewProductVariants = [CreateNewVariant(position: 1, sku: "sku-1")],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(request.RetainedProductVariants)
                && error.ErrorMessage.Contains("SKUs")
        );
    }

    [Fact]
    public async Task ExistingVariantRequiresExactlyOneValidImage()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants =
            [
                CreateExistingVariant() with
                {
                    ExistingImage = new ExistingProductVariantImageRequest(
                        "",
                        new string('a', 201)
                    ),
                    NewImage = new CreateProductVariantImageRequest { File = CreateFile() },
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".ExistingImage"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Path"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Alt"));
    }

    [Fact]
    public async Task ExistingVariantRejectsMissingImage()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants =
            [
                CreateExistingVariant() with
                {
                    ExistingImage = null,
                    NewImage = null,
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".ExistingImage"));
    }

    [Fact]
    public async Task ValidatesExistingVariantFields()
    {
        var request = CreateValidVariationRequest() with
        {
            RetainedProductVariants =
            [
                CreateExistingVariant() with
                {
                    Id = "",
                    Name = "",
                    Sku = "",
                    CostPrice = -1,
                    SellingPrice = -1,
                    Position = -1,
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Id"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Name"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Sku"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".CostPrice"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".SellingPrice"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Position"));
    }

    private static UpdateProductRequest CreateValidRequest()
    {
        return new UpdateProductRequest
        {
            Id = "01J00000000000000000000000",
            Name = "Pocky",
            Description = "Chocolate biscuit sticks",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            BaseSku = "POCKY-BASE",
            BaseCostPrice = 100,
            BaseSellingPrice = 150,
            RetainedImages = [CreateExistingImage(position: 0)],
        };
    }

    private static UpdateProductRequest CreateValidVariationRequest()
    {
        return CreateValidRequest() with
        {
            HasVariation = true,
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            RetainedProductVariants = [CreateExistingVariant()],
        };
    }

    private static RetainedProductImageRequest CreateExistingImage(int position)
    {
        return new RetainedProductImageRequest($"products/pocky-{position}.jpg", "Pocky", position);
    }

    private static RetainedProductVariantRequest CreateExistingVariant(
        int position = 0,
        string sku = "SKU-1"
    )
    {
        return new RetainedProductVariantRequest
        {
            Id = "01J00000000000000000000001",
            Name = "Chocolate",
            Sku = sku,
            CostPrice = 100,
            SellingPrice = 150,
            Position = position,
            ExistingImage = new ExistingProductVariantImageRequest(
                "products/variants/chocolate.jpg",
                "Chocolate"
            ),
        };
    }

    private static CreateProductVariantRequest CreateNewVariant(int position, string sku)
    {
        return new CreateProductVariantRequest
        {
            Name = "Strawberry",
            Sku = sku,
            CostPrice = 100,
            SellingPrice = 150,
            Position = position,
            ProductVariantImage = new CreateProductVariantImageRequest { File = CreateFile() },
        };
    }

    private static IFormFile CreateFile(long length = 1)
    {
        var stream = new MemoryStream(new byte[Math.Max(length, 1)]);
        return new FormFile(stream, 0, length, "file", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };
    }
}
