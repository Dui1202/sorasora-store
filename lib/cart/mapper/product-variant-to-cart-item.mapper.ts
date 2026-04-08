import { Prisma } from "@/app/generated/prisma/client";
import { cartItemSchema, type CartItem } from "../schema/cart-item.schema";

export type ProductVariantForCartItem = Prisma.ProductVariantGetPayload<{
  include: {
    image: true;
    product: {
      select: {
        id: true;
        name: true;
        sellMode: true;
        preorderReleaseAt: true;
      };
    };
  };
}>;

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
