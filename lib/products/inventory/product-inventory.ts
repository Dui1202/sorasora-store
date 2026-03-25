import { AppError } from "@/core/errors/errors";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";

export type ProductInventoryState = {
  warehouseId?: string;
  variantQuantityById: Map<string, number>;
};

export function getProductInventoryState(
  product: ProductWithRelations,
): ProductInventoryState {
  const warehouseIds = new Set<string>();
  const variantQuantityById = new Map<string, number>();

  for (const variant of product.variants) {
    if (variant.stocks.length > 1) {
      throw new AppError(
        "INVALID_PRODUCT_STOCK_STATE",
        "Product variants must use a single warehouse in the product form flow.",
        409,
        {
          productId: product.id,
          variantId: variant.id,
        },
      );
    }

    const stock = variant.stocks[0];

    if (stock) {
      warehouseIds.add(stock.warehouseId);
      variantQuantityById.set(variant.id, stock.quantity);
      continue;
    }

    variantQuantityById.set(variant.id, 0);
  }

  if (warehouseIds.size > 1) {
    throw new AppError(
      "INVALID_PRODUCT_STOCK_STATE",
      "Product variants must use a single warehouse in the product form flow.",
      409,
      {
        productId: product.id,
        warehouseIds: [...warehouseIds],
      },
    );
  }

  return {
    warehouseId: [...warehouseIds][0],
    variantQuantityById,
  };
}
