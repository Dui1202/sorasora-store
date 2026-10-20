using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace SoraSora.Infrastructure.Persistence;

// Keep these values aligned with the database; changing a value requires a migration.
public static class DatabaseConstraintNames
{
    public static class Warehouses
    {
        public const string Name = "UX_Warehouses_Name";
        public const string SingleDefault = "UX_Warehouses_SingleDefault";
    }

    public static class InventoryItems
    {
        public const string ProductVariantId = "UX_InventoryItems_ProductVariantId";
    }

    public static class Stocks
    {
        public const string OnHandedQuantity = "CK_Stock_OnHandedQuantity";
        public const string ReservedQuantity = "CK_Stock_ReservedQuantity";
    }

    public static class ProductVariants
    {
        public const string CostPrice = "CK_ProductVariant_CostPrice";
        public const string SellingPrice = "CK_ProductVariant_SellingPrice";
        public const string Position = "CK_ProductVariant_Position";
    }

    public static class ProductImages
    {
        public const string Position = "CK_ProductImage_Position";
    }

    public static class OrderItems
    {
        public const string SellingPrice = "CK_OrderItems_SellingPrice";
        public const string Quantity = "CK_OrderItems_Quantity";
        public const string SaleType = "CK_OrderItems_WarehouseId_SaleType";
    }
}
