using FluentValidation;
using Microsoft.AspNetCore.Http;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Contracts.Validators;
using SoraSora.Modules.Products.Domain;

namespace UnitTests.Products;

public sealed class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new(
        new CreateProductImageRequestValidator(),
        new CreateProductVariantRequestValidator(new CreateProductVariantImageRequestValidator())
    );

    [Fact]
    public async Task RejectsProductNameLongerThan100Characters()
    {
        var request = CreateValidRequest() with { Name = new string('a', 101) };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Name));
    }

    [Fact]
    public async Task RejectsDescriptionLongerThan2000Characters()
    {
        var request = CreateValidRequest() with { Description = new string('a', 2001) };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(request.Description));
    }

    [Fact]
    public async Task AcceptsProductAtNameAndDescriptionLimits()
    {
        var request = CreateValidRequest() with
        {
            Name = new string('a', 100),
            Description = new string('a', 2000),
        };

        var result = await _validator.ValidateAsync(request);

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == nameof(request.Name));
        Assert.DoesNotContain(
            result.Errors,
            error => error.PropertyName == nameof(request.Description)
        );
    }

    [Fact]
    public async Task RejectsProductWithVariationsAndNoVariants()
    {
        var request = CreateValidRequest() with
        {
            HasVariation = true,
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            ProductVariants = [],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.ProductVariants)
        );
    }

    [Fact]
    public async Task RejectsInvalidVariantAndVariantImage()
    {
        var request = CreateValidRequest() with
        {
            HasVariation = true,
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            ProductVariants =
            [
                new CreateProductVariantRequest
                {
                    Name = "",
                    Sku = "",
                    CostPrice = -1,
                    SellingPrice = -1,
                    Position = -1,
                    ProductVariantImage = new CreateProductVariantImageRequest
                    {
                        File = CreateFile(length: 0),
                    },
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Name"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Sku"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".CostPrice"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".SellingPrice"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".Position"));
        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(".File"));
    }

    [Fact]
    public async Task RejectsDuplicateVariantPositions()
    {
        var variant = CreateValidVariant();
        var request = CreateValidRequest() with
        {
            HasVariation = true,
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            ProductVariants = [variant, variant with { Sku = "SKU-2" }],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.ProductVariants)
        );
    }

    [Fact]
    public async Task RejectsDuplicateVariantSkusIgnoringCase()
    {
        var variant = CreateValidVariant();
        var request = CreateValidRequest() with
        {
            HasVariation = true,
            BaseSku = null,
            BaseCostPrice = null,
            BaseSellingPrice = null,
            ProductVariants =
            [
                variant,
                variant with
                {
                    Sku = "sku-1",
                    Position = 1,
                },
            ],
        };

        var result = await _validator.ValidateAsync(request);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == nameof(request.ProductVariants)
        );
    }

    private static CreateProductRequest CreateValidRequest()
    {
        return new CreateProductRequest
        {
            Name = "Pocky",
            Description = "Chocolate biscuit sticks",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            BaseSku = "POCKY-BASE",
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

    private static CreateProductVariantRequest CreateValidVariant()
    {
        return new CreateProductVariantRequest
        {
            Name = "Chocolate",
            Sku = "SKU-1",
            CostPrice = 100,
            SellingPrice = 150,
            Position = 0,
            ProductVariantImage = new CreateProductVariantImageRequest
            {
                File = CreateFile(),
            },
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
