namespace SoraSora.Modules.Inventory.Domain
{
    public sealed class Stock
    {
        public required string Id { get; set; }
        public required string ProductVariantId { get; set; }
        public required string WarehouseId { get; set; }
        public required int Quantity { get; set; } = 0;
    }
}
