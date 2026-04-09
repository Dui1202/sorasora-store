import { PreorderStage, SellMode } from "@/app/generated/prisma/browser";
import { CartItem } from "@/lib/cart/schema/cart-item.schema";
import { OrderItem, orderItemSchema } from "@/lib/orders/schema/order-item.schema";

/**
 * Maps the cart item checkout snapshot into the persisted order item shape.
 *
 * The mapper keeps order item data detached from mutable cart and catalog state
 * by copying the current cart previews into the order snapshot fields.
 */
export function cartItemToOrderItemMapper(
  orderId: string,
  cartItem: CartItem,
): OrderItem {
  const unitPriceAmount = cartItem.unitPricePreviewAmount;
  const lineSubtotalAmount =
    unitPriceAmount !== undefined ? unitPriceAmount * cartItem.quantity : undefined;
  const lineTotalAmount =
    cartItem.lineTotalPreviewAmount ?? lineSubtotalAmount;
  const lineDiscountAmount =
    lineSubtotalAmount !== undefined && lineTotalAmount !== undefined
      ? Math.max(lineSubtotalAmount - lineTotalAmount, 0)
      : undefined;

  return orderItemSchema.parse({
    orderId,
    productId: cartItem.productId,
    variantId: cartItem.variantId,
    productNameSnapshot: cartItem.productNamePreview,
    variantNameSnapshot: cartItem.variantNamePreview,
    skuSnapshot: cartItem.skuPreview,
    imageUrlSnapshot: cartItem.imagePathPreview,
    sellModeSnapshot: cartItem.sellModePreview,
    releaseDateSnapshot: cartItem.preorderReleaseDatePreview,
    preorderStage:
      cartItem.sellModePreview === SellMode.PREORDER
        ? PreorderStage.AWAITING_RELEASE
        : PreorderStage.NONE,
    unitPriceAmount,
    quantityOrdered: cartItem.quantity,
    quantityCancelled: 0,
    quantityFulfilled: 0,
    quantityRefunded: 0,
    lineSubtotalAmount,
    lineDiscountAmount,
    lineTotalAmount,
    requiresShipping: true,
    weightGramsSnapshot: undefined,
    taxRateSnapshot: undefined,
  });
}
