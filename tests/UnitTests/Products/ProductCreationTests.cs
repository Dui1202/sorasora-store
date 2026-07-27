using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Requests.Create;
using SoraSora.Modules.Products.Domain;

namespace UnitTests.Products;

public sealed class ProductCreationTests
{
    [Fact]
    public void CreateProductRequestDoesNotExposeArchiveState()
    {
        Assert.Null(typeof(CreateProductRequest).GetProperty(nameof(Product.IsArchived)));
    }

    [Fact]
    public void CreateProductVariantRequestDoesNotExposeArchiveState()
    {
        Assert.Null(
            typeof(CreateProductVariantRequest).GetProperty(nameof(ProductVariant.IsArchived))
        );
    }

    [Fact]
    public void CreateProductCommandDoesNotExposeArchiveState()
    {
        Assert.Null(typeof(CreateProductCommand).GetProperty(nameof(Product.IsArchived)));
    }

    [Fact]
    public void CreateProductVariantCommandDoesNotExposeArchiveState()
    {
        Assert.Null(
            typeof(CreateProductVariantCommand).GetProperty(nameof(ProductVariant.IsArchived))
        );
    }

    [Fact]
    public void ProductIsNotArchivedWhenCreated()
    {
        var product = Product.Create(
            name: "Pocky",
            description: null,
            supplierCountry: SupplierCountry.JAPAN,
            sellMode: SellMode.IN_STOCK,
            isPublished: false,
            hasVariation: false
        );

        Assert.False(product.IsArchived);
    }

    [Fact]
    public void ProductVariantIsNotArchivedWhenCreated()
    {
        var productVariant = ProductVariant.Create(
            name: "Chocolate",
            sku: "POCKY-CHOCO",
            costPrice: 100,
            sellingPrice: 150,
            position: 0
        );

        Assert.False(productVariant.IsArchived);
    }
}
