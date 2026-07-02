using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariant : IAuditable
    {
        public required string Id { get; set; }
        public required string ProductId { get; set; }
        public required string Sku { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
        public required DateTimeOffset UpdatedAt { get; set; }
        public required string Name { get; set; }
        public required int CostPrice { get; set; }
        public required int SellingPrice { get; set; }
        public required Product Product { get; set; }
        public required int Position { get; set; } = 0;
        public required bool IsArchived { get; set; } = false;
        public required bool IsFullPaymentAllowed { get; set; } = true;
        public required bool IsDepositRequired { get; set; } = false;
        public int? DepositPrice { get; set; } = 0;
    }
}
