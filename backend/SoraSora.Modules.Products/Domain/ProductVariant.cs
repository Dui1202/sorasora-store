using NUlid;
using SoraSora.Shared.Interface;

namespace SoraSora.Modules.Products.Domain
{
    public sealed class ProductVariant : IAuditable
    {
        public string Id { get; private set; } = Ulid.NewUlid().ToString();
        public string ProductId { get; private set; } = null!;

        // public required string Sku { get; set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public string Name { get; private set; } = null!;
        public int CostPrice { get; private set; }
        public int SellingPrice { get; private set; }
        public Product Product { get; private set; } = null!;
        public int Position { get; private set; } = 0;
        public bool IsArchived { get; private set; }
        public ProductVariantImage? ProductVariantImage { get; private set; }
        // public required bool IsFullPaymentAllowed { get; set; } = true;
        // public required bool IsDepositRequired { get; set; } = false;
        // public int? DepositPrice { get; set; } = 0;
    }
}
