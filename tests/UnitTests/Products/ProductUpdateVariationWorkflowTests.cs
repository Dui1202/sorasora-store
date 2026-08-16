using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Infrastructure.Persistence.Interface;
using SoraSora.Modules.Media.Application;
using SoraSora.Modules.Media.Contracts.Upload;
using SoraSora.Modules.Media.Domain.Upload;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Commands.Update;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Infrastructure.Persistence;

namespace UnitTests.Products;

public sealed class ProductUpdateVariationWorkflowTests
{
    [Fact]
    public async Task ReconcilesVariationOptionsAndRebuildsRetainedVariantSelections()
    {
        await using var database = await ProductTestDatabase.CreateAsync();
        var seeded = await database.SeedProductAsync();
        var service = database.CreateProductService();

        await service.UpdateProductAsync(
            CreateUpdateCommand(seeded) with
            {
                Variations =
                [
                    new UpdateVariationCommand
                    {
                        Id = seeded.VariationId,
                        ClientId = "flavor",
                        Name = "Flavor",
                        Position = 0,
                        VariationOptions =
                        [
                            new UpdateVariationOptionCommand
                            {
                                Id = seeded.OptionId,
                                ClientId = "strawberry",
                                Value = "Berry",
                                Position = 0,
                            },
                            new UpdateVariationOptionCommand
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
                    CreateRetainedVariant(seeded) with
                    {
                        ProductVariantOptions =
                        [
                            new CreateProductVariantOptionCommand
                            {
                                VariationClientId = "flavor",
                                VariationOptionClientId = "chocolate",
                            },
                        ],
                    },
                ],
            },
            CancellationToken.None
        );

        database.Context.ChangeTracker.Clear();
        var product = await database.Context
            .Set<Product>()
            .AsSplitQuery()
            .Include(x => x.Variations)
                .ThenInclude(x => x.VariationOptions)
            .Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductVariantOptions)
                    .ThenInclude(x => x.VariationOption)
            .SingleAsync();

        var variation = Assert.Single(product.Variations);
        Assert.Equal(seeded.VariationId, variation.Id);
        Assert.Collection(
            variation.VariationOptions.OrderBy(x => x.Position),
            option => Assert.Equal("Berry", option.Value),
            option => Assert.Equal("Chocolate", option.Value)
        );
        Assert.Equal(
            "Chocolate",
            Assert.Single(Assert.Single(product.ProductVariants).ProductVariantOptions)
                .VariationOption.Value
        );
    }

    [Fact]
    public async Task EmptyVariationGraphRemovesVariationsAndSelections()
    {
        await using var database = await ProductTestDatabase.CreateAsync();
        var seeded = await database.SeedProductAsync();
        var service = database.CreateProductService();

        await service.UpdateProductAsync(
            CreateUpdateCommand(seeded) with
            {
                Variations = [],
                RetainedProductVariants = [CreateRetainedVariant(seeded)],
            },
            CancellationToken.None
        );

        database.Context.ChangeTracker.Clear();
        var product = await database.Context
            .Set<Product>()
            .Include(x => x.Variations)
            .Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductVariantOptions)
            .SingleAsync();

        Assert.Empty(product.Variations);
        Assert.Empty(Assert.Single(product.ProductVariants).ProductVariantOptions);
    }

    [Fact]
    public async Task AddsNewVariationAndLinksItToRetainedVariant()
    {
        await using var database = await ProductTestDatabase.CreateAsync();
        var seeded = await database.SeedProductAsync();
        var service = database.CreateProductService();

        await service.UpdateProductAsync(
            CreateUpdateCommand(seeded) with
            {
                Variations =
                [
                    new UpdateVariationCommand
                    {
                        Id = seeded.VariationId,
                        ClientId = "flavor",
                        Name = "Flavor",
                        Position = 0,
                        VariationOptions =
                        [
                            new UpdateVariationOptionCommand
                            {
                                Id = seeded.OptionId,
                                ClientId = "strawberry",
                                Value = "Strawberry",
                                Position = 0,
                            },
                        ],
                    },
                    new UpdateVariationCommand
                    {
                        ClientId = "size",
                        Name = "Size",
                        Position = 1,
                        VariationOptions =
                        [
                            new UpdateVariationOptionCommand
                            {
                                ClientId = "large",
                                Value = "Large",
                                Position = 0,
                            },
                        ],
                    },
                ],
                RetainedProductVariants =
                [
                    CreateRetainedVariant(seeded) with
                    {
                        ProductVariantOptions =
                        [
                            new CreateProductVariantOptionCommand
                            {
                                VariationClientId = "flavor",
                                VariationOptionClientId = "strawberry",
                            },
                            new CreateProductVariantOptionCommand
                            {
                                VariationClientId = "size",
                                VariationOptionClientId = "large",
                            },
                        ],
                    },
                ],
            },
            CancellationToken.None
        );

        database.Context.ChangeTracker.Clear();
        var product = await database.Context
            .Set<Product>()
            .AsSplitQuery()
            .Include(x => x.Variations)
            .Include(x => x.ProductVariants)
                .ThenInclude(x => x.ProductVariantOptions)
                    .ThenInclude(x => x.VariationOption)
            .SingleAsync();

        Assert.Equal(2, product.Variations.Count);
        Assert.Equal(2, Assert.Single(product.ProductVariants).ProductVariantOptions.Count);
    }

    [Fact]
    public async Task SwapsRetainedOptionValuesAndPositionsWithoutUniqueIndexConflicts()
    {
        await using var database = await ProductTestDatabase.CreateAsync();
        var seeded = await database.SeedProductAsync();
        var service = database.CreateProductService();

        await service.UpdateProductAsync(
            CreateUpdateCommand(seeded) with
            {
                Variations =
                [
                    new UpdateVariationCommand
                    {
                        Id = seeded.VariationId,
                        ClientId = "flavor",
                        Name = "Flavor",
                        Position = 0,
                        VariationOptions =
                        [
                            new UpdateVariationOptionCommand
                            {
                                Id = seeded.OptionId,
                                ClientId = "chocolate",
                                Value = "Chocolate",
                                Position = 1,
                            },
                            new UpdateVariationOptionCommand
                            {
                                Id = seeded.SecondOptionId,
                                ClientId = "strawberry",
                                Value = "Strawberry",
                                Position = 0,
                            },
                        ],
                    },
                ],
                RetainedProductVariants =
                [
                    CreateRetainedVariant(seeded) with
                    {
                        ProductVariantOptions =
                        [
                            new CreateProductVariantOptionCommand
                            {
                                VariationClientId = "flavor",
                                VariationOptionClientId = "strawberry",
                            },
                        ],
                    },
                ],
            },
            CancellationToken.None
        );

        database.Context.ChangeTracker.Clear();
        var options = await database.Context
            .Set<VariationOption>()
            .OrderBy(x => x.Position)
            .ToListAsync();

        Assert.Equal(seeded.SecondOptionId, options[0].Id);
        Assert.Equal("Strawberry", options[0].Value);
        Assert.Equal(seeded.OptionId, options[1].Id);
        Assert.Equal("Chocolate", options[1].Value);
    }

    [Fact]
    public async Task RejectsVariationIdsOwnedByAnotherProduct()
    {
        await using var database = await ProductTestDatabase.CreateAsync();
        var seeded = await database.SeedProductAsync();
        var service = database.CreateProductService();
        var command = CreateUpdateCommand(seeded) with
        {
            Variations =
            [
                new UpdateVariationCommand
                {
                    Id = "foreign-variation",
                    ClientId = "flavor",
                    Name = "Flavor",
                    Position = 0,
                    VariationOptions =
                    [
                        new UpdateVariationOptionCommand
                        {
                            ClientId = "strawberry",
                            Value = "Strawberry",
                            Position = 0,
                        },
                    ],
                },
            ],
            RetainedProductVariants =
            [
                CreateRetainedVariant(seeded) with
                {
                    ProductVariantOptions =
                    [
                        new CreateProductVariantOptionCommand
                        {
                            VariationClientId = "flavor",
                            VariationOptionClientId = "strawberry",
                        },
                    ],
                },
            ],
        };

        var exception = await Assert.ThrowsAsync<ProductVariationOwnershipException>(() =>
            service.UpdateProductAsync(command, CancellationToken.None)
        );

        Assert.Equal(ProductErrorCodes.VariationNotOwned, exception.Code);
    }

    private static UpdateProductCommand CreateUpdateCommand(SeededProduct seeded) =>
        new()
        {
            Id = seeded.ProductId,
            Version = seeded.Version,
            Name = "Pocky",
            SupplierCountry = SupplierCountry.JAPAN,
            SellMode = SellMode.IN_STOCK,
            RetainedImages =
            [
                new RetainedProductImageCommand("asset-1", null, 0),
            ],
        };

    private static RetainedProductVariantCommand CreateRetainedVariant(SeededProduct seeded) =>
        new()
        {
            Id = seeded.VariantId,
            Name = "Strawberry",
            Sku = "STRAWBERRY",
            CostPrice = 100,
            SellingPrice = 150,
            Position = 0,
        };

    private sealed record SeededProduct(
        string ProductId,
        Guid Version,
        string VariantId,
        string VariationId,
        string OptionId,
        string SecondOptionId
    );

    private sealed class ProductTestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private ProductTestDatabase(SqliteConnection connection, SoraSoraDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public SoraSoraDbContext Context { get; }

        public static async Task<ProductTestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<SoraSoraDbContext>()
                .UseSqlite(connection)
                .Options;
            IModelBuilderContributor[] contributors = [
                new ProductsModuleModelBuilderContributor(),
            ];
            var context = new SoraSoraDbContext(options, contributors);
            await context.Database.EnsureCreatedAsync();
            return new ProductTestDatabase(connection, context);
        }

        public ProductService CreateProductService() =>
            new(Context, new FakeImageMediaService(), NullLogger<ProductService>.Instance);

        public async Task<SeededProduct> SeedProductAsync()
        {
            var product = Product.Create(
                "Pocky",
                null,
                SupplierCountry.JAPAN,
                SellMode.IN_STOCK,
                false
            );
            product.AddImage(ProductImage.Create("asset-1", 0));

            var variation = Variation.Create("Flavor", 0);
            var option = VariationOption.Create("Strawberry", 0);
            var secondOption = VariationOption.Create("Chocolate", 1);
            variation.AddOption(option);
            variation.AddOption(secondOption);
            product.AddVariation(variation);

            var variant = ProductVariant.Create("Strawberry", "STRAWBERRY", 100, 150, 0);
            variant.AddProductVariantOption(ProductVariantOption.Create(variant, option));
            product.AddProductVariant(variant);

            Context.Set<Product>().Add(product);
            await Context.SaveChangesAsync();

            var seeded = new SeededProduct(
                product.Id,
                product.Version,
                variant.Id,
                variation.Id,
                option.Id,
                secondOption.Id
            );
            Context.ChangeTracker.Clear();
            return seeded;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FakeImageMediaService : IImageMediaService
    {
        public Task<IReadOnlyList<MediaUploadResult>> UploadImagesAsync(
            IReadOnlyCollection<ImageUploadJob> jobs,
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<MediaUploadResult>>([]);

        public Task ScheduleImageDeletionAsync(
            IReadOnlyCollection<string> mediaAssetIds,
            CancellationToken cancellationToken
        ) => Task.CompletedTask;

        public Task UpdateImageAltTextsAsync(
            IReadOnlyDictionary<string, string?> altTextByMediaAssetId,
            CancellationToken cancellationToken
        ) => Task.CompletedTask;
    }
}
