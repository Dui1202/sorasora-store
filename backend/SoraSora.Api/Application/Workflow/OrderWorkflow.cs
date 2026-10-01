using SoraSora.Api.Contracts.Commands.OrderWorkflow;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Inventory.Application.Services;
using SoraSora.Modules.Orders.Application.Services;
using SoraSora.Modules.Orders.Contracts.Commands;
using SoraSora.Modules.Orders.Contracts.Responses;
using SoraSora.Modules.Products.Application.Services;

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

            var createOrderCommand = await InstantiateCreateOrderCommand(
                command,
                cancellationToken
            );

            var order = await _orderService.CreateOrderAsync(createOrderCommand, cancellationToken);

            foreach (var orderItem in order.OrderItems)
            {
                await _inventoryService.ReserveStockAsync(
                    productVariantId: orderItem.ProductVariantId,
                    warehouseId: command.WarehouseId,
                    quantity: orderItem.Quantity,
                    description: $"Reserve stock for order #{order.Id}.",
                    cancellationToken
                );
            }

            await transaction.CommitAsync(cancellationToken);

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

            // var defaultWarehouse = await _warehouseService.GetDefaultWarehouseAsync(
            //     cancellationToken
            // );

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

        private async Task<CreateOrderCommand> InstantiateCreateOrderCommand(
            PlaceOrderWorkflowCommand command,
            CancellationToken cancellationToken
        )
        {
            var createOrderItemCommandList = new List<CreateOrderItemCommand>();

            foreach (var placeOrderItem in command.OrderItems)
            {
                var productVariant = await _productService.GetProductVariantByIdAsync(
                    placeOrderItem.ProductVariantId,
                    cancellationToken
                );

                var createOrderItemCommand = new CreateOrderItemCommand
                {
                    ProductName = productVariant.Name,
                    ProductVariantId = productVariant.Id,
                    Quantity = placeOrderItem.Quantity,
                    SellingPrice = productVariant.SellingPrice,
                    Sku = productVariant.Sku,
                };

                createOrderItemCommandList.Add(createOrderItemCommand);
            }

            return new CreateOrderCommand
            {
                WarehouseId = command.WarehouseId,
                CustomerName = command.CustomerName,
                CustomerPhoneNumber = command.CustomerPhoneNumber,
                CustomerEmail = command.CustomerEmail,
                CustomerNote = command.CustomerNote,
                ShippingAddress = command.ShippingAddress,
                StoreNote = command.StoreNote,
                OrderItems = createOrderItemCommandList,
            };
        }
    }
}
