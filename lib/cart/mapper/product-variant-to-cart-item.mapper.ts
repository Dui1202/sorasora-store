import { cartItemSchema, type CartItem } from "../schema/cart-item.schema";
import { ProductVariantForCartItem } from "../type";

/**
 * Maps a product variant and its related product snapshot into the cart item
 * write shape used by cart workflows.
 */
export function productVariantToCartItemMapper(
  cartId: string,
  variant: ProductVariantForCartItem,
  quantity: number,
): CartItem {
  const unitPricePreviewAmount = variant.sellingPrice;
  const lineTotalPreviewAmount = unitPricePreviewAmount * quantity;

  return cartItemSchema.parse({
    cartId,
    productId: variant.productId,
    variantId: variant.id,
    quantity,
    isSelected: true,
    productNamePreview: variant.product.name,
    variantNamePreview: variant.name,
    skuPreview: variant.sku,
    imagePathPreview: variant.image?.path,
    unitPricePreviewAmount,
    lineTotalPreviewAmount,
    sellModePreview: variant.product.sellMode,
    preorderReleaseDatePreview: variant.product.preorderReleaseAt ?? undefined,
  });
}
