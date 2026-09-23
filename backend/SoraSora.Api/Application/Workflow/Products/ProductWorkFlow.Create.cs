using SoraSora.Api.Mappers;
using SoraSora.Modules.Products.Contracts.Commands.Create;
using SoraSora.Modules.Products.Contracts.Responses;
using SoraSora.Modules.Products.Domain;
using SoraSora.Modules.Products.Exceptions;
using SoraSora.Modules.Products.Mappers;

namespace SoraSora.Api.Application.Workflow.Products;

public sealed partial class ProductWorkflow
{
    public async Task<ProductResponse> CreateProductAsync(
        CreateProductWorkflowCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var variantsByClientId = ValidateAndIndexVariants(command);
        var productCommand = CreateProductWorkflowMapper.ToCreateProductCommand(command);
        Product? product = null;
        var uploadedMediaAssetIds = new HashSet<string>(StringComparer.Ordinal);

        var commitAttempted = false;
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            //Handle Product operations
            product = await _productService.CreateProductAsync(productCommand, cancellationToken);

            //Handle ImageMedia operations
            await UploadAndAttachProductImageFilesAsync(
                product,
                command,
                uploadedMediaAssetIds,
                cancellationToken
            );

            //Handle Inventory operations
            await CreateInventoryItemsForProductVariantsAsync(
                product,
                variantsByClientId,
                cancellationToken
            );

            cancellationToken.ThrowIfCancellationRequested();
            commitAttempted = true;
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created product {ProductId} and its inventory", product.Id);

            return ProductDtoMapper.ToProductResponse(product);
        }
        catch
        {
            await TryRollbackAndCleanUpAsync(product, uploadedMediaAssetIds, commitAttempted);
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
                _logger.LogError(exception, "Failed to dispose the create-product transaction");
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
            if (
                string.IsNullOrWhiteSpace(variant.ClientId)
                || !variantsByClientId.TryAdd(variant.ClientId, variant)
            )
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

    private async Task CreateInventoryItemsForProductVariantsAsync(
        Product product,
        IDictionary<string, CreateProductVariantWorkflowCommand> variantsByClientId,
        CancellationToken cancellationToken
    )
    {
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
            await _inventoryService.CreateInventoryItemAsync(inventoryCommand, cancellationToken);
        }

        if (variantsByClientId.Count > 0)
        {
            throw new InvalidOperationException(
                "The product service did not create every workflow variant."
            );
        }
    }

    private async Task TryRollbackAndCleanUpAsync(
        Product? product,
        ISet<string> uploadedMediaAssetIds,
        bool commitAttempted
    )
    {
        try
        {
            if (_dbContext.Database.CurrentTransaction is { } transaction)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to roll back product creation for {ProductId}",
                product?.Id
            );
        }

        if (commitAttempted)
        {
            _logger.LogError(
                "Create-product commit outcome is uncertain for {ProductId}; media cleanup was skipped",
                product?.Id
            );

            return;
        }

        if (uploadedMediaAssetIds.Count == 0)
        {
            return;
        }

        try
        {
            await _imageMediaService.ScheduleFailedUploadCleanupAsync(
                uploadedMediaAssetIds.ToList(),
                CancellationToken.None
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to schedule cleanup for uploaded media {MediaAssetIds}",
                uploadedMediaAssetIds
            );
        }
    }

    private async Task UploadAndAttachProductImageFilesAsync(
        Product product,
        CreateProductWorkflowCommand command,
        ISet<string> uploadedMediaAssetIds,
        CancellationToken cancellationToken
    )
    {
        var productImageUploadJobs = BuildProductImageUploadJobsForCreateCommand(command, product);

        var imageUploadJobs = productImageUploadJobs.Select(x => x.Job).ToList();

        var mediaUploadResult = await _imageMediaService.UploadImagesAsync(
            imageUploadJobs,
            cancellationToken
        );

        AttachUploadedMedia(productImageUploadJobs, mediaUploadResult, uploadedMediaAssetIds);
    }
}
