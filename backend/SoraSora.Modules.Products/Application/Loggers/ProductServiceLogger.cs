using Microsoft.Extensions.Logging;
using SoraSora.Modules.Products.Application.Services;

namespace SoraSora.Modules.Products.Application.Loggers;

internal static partial class ProductServiceLogger
{
    [LoggerMessage(
        EventId = 1000,
        EventName = nameof(ProductCreated),
        Level = LogLevel.Information,
        Message = "Created product {ProductName} with ID {ProductId}"
    )]
    public static partial void ProductCreated(
        this ILogger<ProductService> logger,
        string productName,
        string productId
    );

    [LoggerMessage(
        EventId = 1001,
        EventName = nameof(ProductImageCleanupFailed),
        Level = LogLevel.Warning,
        Message = "Failed to delete uploaded product image {ImagePath} during cleanup"
    )]
    public static partial void ProductImageCleanupFailed(
        this ILogger<ProductService> logger,
        Exception exception,
        string imagePath
    );

    [LoggerMessage(
        EventId = 1100,
        EventName = nameof(ProductUpdated),
        Level = LogLevel.Information,
        Message = "Updated product {ProductName} with ID {ProductId}"
    )]
    public static partial void ProductUpdated(
        this ILogger<ProductService> logger,
        string productName,
        string productId
    );

    [LoggerMessage(
        EventId = 1200,
        EventName = nameof(ProductDeleted),
        Level = LogLevel.Information,
        Message = "Deleted product {ProductName} with ID {ProductId}"
    )]
    public static partial void ProductDeleted(
        this ILogger<ProductService> logger,
        string productName,
        string productId
    );
}
