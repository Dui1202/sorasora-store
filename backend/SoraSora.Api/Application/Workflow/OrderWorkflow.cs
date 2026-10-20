using System.Diagnostics;
using SoraSora.Api.Contracts.Commands.OrderWorkflow;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Orders.Application.Services;
using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Orders.Domain;
using SoraSora.Modules.Products.Application.Services;
using SoraSora.Modules.Products.Domain;

namespace SoraSora.Api.Application.Workflow
{
    public sealed class OrderWorkflow(
        SoraSoraDbContext dbContext,
        IOrderService orderService,
        IProductService productService,
        IInventoryService inventoryService,
        IWarehouseService warehouseService
    )
    {
        private readonly SoraSoraDbContext _dbContext = dbContext;
        private readonly IOrderService _orderService = orderService;
        private readonly IInventoryService _inventoryService = inventoryService;
        private readonly IProductService _productService = productService;
        private readonly IWarehouseService _warehouseService = warehouseService;

        public async Task<OrderResponse> PlaceOrderAsync(
            PlaceOrderWorkflowCommand command,
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            var createOrderCommand = await MapPlaceOrderWorkflowCommandToCreateOrderCommandAsync(
                command,
                cancellationToken
            );

            var order = await CreateOrderAndReserveStockAsync(
                createOrderCommand,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            return order;
        }

        public async Task<OrderResponse> CreateOrderAsync(
            CreateOrderWorkflowCommand command,
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            var createOrderCommand = await MapCreateOrderWorkflowCommandToCreateOrderCommandAsync(
                command,
                cancellationToken
            );

            var order = await CreateOrderAndReserveStockAsync(
                createOrderCommand,
                cancellationToken
            );

            await transaction.CommitAsync(cancellationToken);

            return order;
        }

        private async Task<OrderResponse> CreateOrderAndReserveStockAsync(
            CreateOrderCommand command,
            CancellationToken cancellationToken
        )
        {
            var order = await _orderService.CreateOrderAsync(command, cancellationToken);

            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.SaleType == OrderSaleType.IN_STOCK)
                {
                    await _inventoryService.ReserveStockAsync(
                        productVariantId: orderItem.ProductVariantId,
                        warehouseId: orderItem.WarehouseId,
                        quantity: orderItem.Quantity,
                        description: $"Reserve stock for order #{order.Id} and order item #{orderItem.Id}.",
                        cancellationToken
                    );
                }
            }

            return order;
        }

        public async Task<OrderResponse> CancelOrderAsync(
            string orderId,
            string warehouseId,
            Guid version,
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            var order = await _orderService.CancelOrderAsync(orderId, version, cancellationToken);

            foreach (var orderItem in order.OrderItems)
            {
                await _inventoryService.ReleaseStockAsync(
                    productVariantId: orderItem.ProductVariantId,
                    warehouseId: warehouseId,
                    quantity: orderItem.Quantity,
                    description: $"Release stock for cancelled order #{order.Id}",
                    cancellationToken
                );
            }

            await transaction.CommitAsync(cancellationToken);

            return order;
        }

        public async Task<OrderResponse> CompleteOrderAsync(
            string orderId,
            string warehouseId,
            Guid version,
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            var order = await _orderService.CompleteOrderAsync(orderId, version, cancellationToken);

            // var warehouse = await _warehouseService.GetDefaultWarehouseAsync(cancellationToken);

            foreach (var orderItem in order.OrderItems)
            {
                await _inventoryService.SaleStockAsync(
                    productVariantId: orderItem.ProductVariantId,
                    warehouseId: warehouseId,
                    quantity: orderItem.Quantity,
                    description: "Sale stock for completed order.",
                    cancellationToken: cancellationToken
                );
            }

            await transaction.CommitAsync(cancellationToken);

            return order;
        }

        private async Task<CreateOrderCommand> MapCreateOrderWorkflowCommandToCreateOrderCommandAsync(
            CreateOrderWorkflowCommand command,
            CancellationToken cancellationToken
        )
        {
            var orderItems = await MapOrderItemsToCreateOrderItemCommandsAsync(
                command.OrderItems,
                cancellationToken
            );
            return new CreateOrderCommand
            {
                CustomerName = command.CustomerName,
                CustomerPhoneNumber = command.CustomerPhoneNumber,
                CustomerEmail = command.CustomerEmail,
                CustomerNote = command.CustomerNote,
                ShippingAddress = command.ShippingAddress,
                StoreNote = command.StoreNote,
                OrderItems = orderItems,
            };
        }

        private async Task<CreateOrderCommand> MapPlaceOrderWorkflowCommandToCreateOrderCommandAsync(
            PlaceOrderWorkflowCommand command,
            CancellationToken cancellationToken
        )
        {
            var createOrderItemCommandList = await MapOrderItemsToCreateOrderItemCommandsAsync(
                command.OrderItems,
                cancellationToken
            );

            return new CreateOrderCommand
            {
                CustomerName = command.CustomerName,
                CustomerPhoneNumber = command.CustomerPhoneNumber,
                CustomerEmail = command.CustomerEmail,
                CustomerNote = command.CustomerNote,
                ShippingAddress = command.ShippingAddress,
                OrderItems = createOrderItemCommandList,
            };
        }

        private async Task<
            IReadOnlyCollection<CreateOrderItemCommand>
        > MapOrderItemsToCreateOrderItemCommandsAsync(
            IReadOnlyCollection<PlaceOrderItemCommand> orderItems,
            CancellationToken cancellationToken
        )
        {
            var createOrderItemCommandList = new List<CreateOrderItemCommand>();

            var variantIds = orderItems
                .Select(x => x.ProductVariantId)
                .ToHashSet(StringComparer.Ordinal);

            var orderableVariants = await _productService.GetOrderableProductVariantsAsync(
                variantIds,
                cancellationToken
            );

            var defaultWarehouse = await _warehouseService.GetDefaultWarehouseAsync(
                cancellationToken
            );

            var orderItemsByVariantId = orderItems.ToDictionary(x => x.ProductVariantId);

            foreach (var variant in orderableVariants)
            {
                if (!orderItemsByVariantId.TryGetValue(variant.ProductVariantId, out var orderItem))
                {
                    throw new InvalidOperationException();
                }

                var createOrderItemCommand = new CreateOrderItemCommand
                {
                    ProductName = variant.ProductName,
                    ProductVariantName = variant.ProductVariantName,
                    ProductVariantId = variant.ProductVariantId,
                    Quantity = orderItem.Quantity,
                    SaleType = variant.SellMode switch
                    {
                        SellMode.IN_STOCK => OrderSaleType.IN_STOCK,
                        SellMode.PREORDER => OrderSaleType.In_FULFILLMENT,
                        _ => throw new InvalidOperationException(),
                    },
                    SellingPrice = variant.SellingPrice,
                    Sku = variant.Sku,
                    WarehouseId =
                        variant.SellMode == SellMode.IN_STOCK ? defaultWarehouse.Id : null,
                };

                createOrderItemCommandList.Add(createOrderItemCommand);
            }

            return createOrderItemCommandList;
        }
    }
}
