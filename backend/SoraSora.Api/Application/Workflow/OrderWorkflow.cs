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

            var warehouseId = await GetDefaultWarehouseIdAsync(cancellationToken);

            foreach (var orderItem in order.OrderItems)
            {
                await _inventoryService.ReserveStockAsync(
                    productVariantId: orderItem.ProductVariantId,
                    warehouseId: warehouseId,
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
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            var order = await _orderService.CancelOrderAsync(orderId, cancellationToken);

            var warehouseId = await GetDefaultWarehouseIdAsync(cancellationToken);

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
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );
            var order = await _orderService.CompleteOrderAsync(orderId, cancellationToken);

            var warehouseId = await GetDefaultWarehouseIdAsync(cancellationToken);

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

        private async Task<string> GetDefaultWarehouseIdAsync(CancellationToken cancellationToken)
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);
            var defaultWarehouse = warehouses?.SingleOrDefault(warehouse => warehouse.IsDefault)
                ?? throw new InvalidOperationException("The default warehouse is missing");

            return defaultWarehouse.Id;
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
                    ProductVariantId = productVariant.Id,
                    Quantity = placeOrderItem.Quantity,
                    SellingPrice = productVariant.SellingPrice,
                };

                createOrderItemCommandList.Add(createOrderItemCommand);
            }

            return new CreateOrderCommand
            {
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
