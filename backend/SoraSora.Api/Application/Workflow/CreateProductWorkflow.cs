using SoraSora.Api.Mappers;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Media.Application;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Exceptions;

namespace SoraSora.Api.Application.Workflow;

public sealed class CreateProductWorkflow(
    SoraSoraDbContext dbContext,
    IProductService productService,
    IInventoryService inventoryService,
    IImageMediaService imageMediaService,
    ILogger<CreateProductWorkflow> logger
)
{
    public async Task<ProductResponse> CreateProductAsync(
        CreateProductWorkflowCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var variantsByClientId = ValidateAndIndexVariants(command);
        var productCommand = CreateProductWorkflowMapper.ToCreateProductCommand(command);
        ProductResponse? product = null;
        var commitAttempted = false;
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            product = await productService.CreateProductAsync(productCommand, cancellationToken);

            foreach (var variant in product.ProductVariants)
            {
                if (!variantsByClientId.Remove(variant.ClientId, out var variantCommand))
                {
                    throw new InvalidOperationException(
                        "The created product variant does not match a workflow variant."
                    );
                }

                var inventoryCommand = CreateProductWorkflowMapper.ToCreateInventoryItemCommand(
                    variant,
                    variantCommand
                );
                await inventoryService.CreateInventoryItemAsync(inventoryCommand, cancellationToken);
            }

            if (variantsByClientId.Count > 0)
            {
                throw new InvalidOperationException(
                    "The product service did not create every workflow variant."
                );
            }

            cancellationToken.ThrowIfCancellationRequested();
            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Created product {ProductId} and its inventory", product.Id);
            return product;
        }
        catch
        {
            await TryRollbackAndCleanUpAsync(product, commitAttempted);
            throw;
        }
        finally
        {
            try
            {
                await transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                // Disposal must not replace the workflow failure or turn a committed write
                // into a failed response that invites a duplicate submission.
                logger.LogError(exception, "Failed to dispose the create-product transaction");
            }
        }
    }

    private static Dictionary<string, CreateProductVariantWorkflowCommand> ValidateAndIndexVariants(
        CreateProductWorkflowCommand command
    )
    {
        if (command.ProductVariants is null || command.ProductVariants.Count == 0)
        {
            throw new ProductValidationException("At least one product variant is required.");
        }

        var variantsByClientId = new Dictionary<string, CreateProductVariantWorkflowCommand>(
            StringComparer.Ordinal
        );
        foreach (var variant in command.ProductVariants)
        {
            if (string.IsNullOrWhiteSpace(variant.ClientId)
                || !variantsByClientId.TryAdd(variant.ClientId, variant))
            {
                throw new ProductValidationException(
                    "Product variant client IDs must be nonempty and unique."
                );
            }

            if (variant.InitialQuantity < 0)
            {
                throw new ProductValidationException("Initial quantity cannot be negative.");
            }
        }

        return variantsByClientId;
    }

    private async Task TryRollbackAndCleanUpAsync(ProductResponse? product, bool commitAttempted)
    {
        try
        {
            // ProductService may already have rolled back and disposed this transaction
            // before saving cleanup for a failure inside product creation.
            if (dbContext.Database.CurrentTransaction is { } transaction)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                await transaction.DisposeAsync();
            }

            dbContext.ChangeTracker.Clear();

            if (commitAttempted)
            {
                // A lost commit response can mean the write succeeded. Never delete its images.
                logger.LogError(
                    "Create-product commit outcome is uncertain for {ProductId}; media cleanup was skipped",
                    product?.Id
                );
                return;
            }

            if (product is null)
            {
                return;
            }

            var mediaAssetIds = product.ProductImages
                .Select(image => image.MediaAssetId)
                .Concat(product.ProductVariants
                    .Where(variant => variant.ProductVariantImage is not null)
                    .Select(variant => variant.ProductVariantImage!.MediaAssetId))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            await imageMediaService.ScheduleImageDeletionAsync(mediaAssetIds, CancellationToken.None);
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to roll back or clean up product creation for {ProductId}; media may need reconciliation",
                product?.Id
            );
        }
    }
}
