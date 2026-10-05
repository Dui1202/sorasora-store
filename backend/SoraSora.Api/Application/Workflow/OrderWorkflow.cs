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

            var createOrderCommand = await MapPlaceOrderWorkflowCommandToCreateOrderCommandAsync(
                command,
                cancellationToken
            );

            var order = await CreateOrderAndReserveStockAsync(createOrderCommand, cancellationToken);

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

            var order = await CreateOrderAndReserveStockAsync(createOrderCommand, cancellationToken);

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
                await _inventoryService.ReserveStockAsync(
                    productVariantId: orderItem.ProductVariantId,
                    warehouseId: command.WarehouseId,
                    quantity: orderItem.Quantity,
                    description: $"Reserve stock for order #{order.Id}.",
                    cancellationToken
                );
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
                WarehouseId = command.WarehouseId,
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
                WarehouseId = command.WarehouseId,
                CustomerName = command.CustomerName,
                CustomerPhoneNumber = command.CustomerPhoneNumber,
                CustomerEmail = command.CustomerEmail,
                CustomerNote = command.CustomerNote,
                ShippingAddress = command.ShippingAddress,
                OrderItems = createOrderItemCommandList,
            };
        }

        private async Task<IReadOnlyCollection<CreateOrderItemCommand>> MapOrderItemsToCreateOrderItemCommandsAsync(
            IReadOnlyCollection<PlaceOrderItemCommand> orderItems,
            CancellationToken cancellationToken
        )
        {
            var createOrderItemCommandList = new List<CreateOrderItemCommand>();

            foreach (var placeOrderItem in orderItems)
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

            return createOrderItemCommandList;
        }
    }
}
