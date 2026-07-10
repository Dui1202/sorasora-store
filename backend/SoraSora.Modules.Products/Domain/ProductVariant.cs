using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariant : IAuditable
    {
        public string Id { get; set; } = null!;
        public string ProductId { get; set; } = null!;

        // public required string Sku { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string Name { get; set; } = null!;
        public int CostPrice { get; set; }
        public int SellingPrice { get; set; }
        public Product Product { get; set; } = null!;
        public int Position { get; set; } = 0;
        public bool IsArchived { get; set; } = false;
        // public required bool IsFullPaymentAllowed { get; set; } = true;
        // public required bool IsDepositRequired { get; set; } = false;
        // public int? DepositPrice { get; set; } = 0;
    }
}
