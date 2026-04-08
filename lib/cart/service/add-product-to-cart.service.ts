import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/infrastructure/database/prisma";
import { getProductVariantForCartItem } from "@/lib/products/repositories/queries/get-product-variant-for-cart-item.repository";
import { productVariantToCartItemMapper } from "../mapper/product-variant-to-cart-item.mapper";
import { CartItem } from "../schema/cart-item.schema";
import { CartOwnerContext } from "../type";
import { getCartService } from "./get-cart.service";

/**
 * Adds a product variant to the current cart for the provided owner context.
 *
 * If the cart does not exist yet, it will be created for the current owner.
 * If the variant is already present in the cart, the quantity is merged into
 * the existing cart item instead of creating a duplicate row.
 */
export async function addProductToCartService(
  owner: Exclude<CartOwnerContext, null>,
  variantId: string,
  quantity: number,
) {
  // Load the latest variant snapshot so cart preview fields are derived from
  // current product data instead of trusting client input.
  const variant = await getProductVariantForCartItem(variantId);

  if (!variant) {
    throw new AppError("PRODUCT_NOT_FOUND", "Product variant not found", 404);
  }

  // Reuse the current cart when it exists, otherwise create the owner's first cart.
  const existingCart = await getCartService(owner);
  const cart = existingCart ?? (await createCartForOwner(owner));

  // Map the live variant record into the cart item write shape.
  const cartItem = productVariantToCartItemMapper(cart.id, variant, quantity);

  // Persist the cart item and refresh cart totals in one transaction.
  return addCartItemToCart(cart.id, cartItem);
}

/**
 * Creates a new cart for the provided owner context.
 *
 * The cart is uniquely associated with either a signed-in user or a guest cart token.
 */
async function createCartForOwner(owner: Exclude<CartOwnerContext, null>) {
  return prisma.cart.create({
    data: owner.userId
      ? {
          userId: owner.userId,
        }
      : {
          guestCartToken: owner.guestCartToken,
        },
  });
}

/**
 * Upserts a cart item into the target cart and recalculates cart totals.
 *
 * Existing items are matched by the unique `cartId + variantId` pair and have
 * their quantity merged with the newly requested quantity.
 */
const addCartItemToCart = async (cartId: string, cartItem: CartItem) => {
  return prisma.$transaction(async (tx) => {
    // Check whether the cart already contains this variant so we can merge quantities.
    const existingCartItem = await tx.cartItem.findUnique({
      where: {
        cartId_variantId: {
          cartId,
          variantId: cartItem.variantId,
        },
      },
      select: {
        quantity: true,
      },
    });

    // Recompute the line totals from the latest unit price snapshot and merged quantity.
    const nextQuantity = (existingCartItem?.quantity ?? 0) + cartItem.quantity;
    const unitPricePreviewAmount = cartItem.unitPricePreviewAmount ?? 0;
    const lineTotalPreviewAmount = unitPricePreviewAmount * nextQuantity;

    // Either create the new cart line or refresh the existing one with the latest snapshot fields.
    await tx.cartItem.upsert({
      where: {
        cartId_variantId: {
          cartId,
          variantId: cartItem.variantId,
        },
      },
      create: {
        ...cartItem,
        quantity: nextQuantity,
        lineTotalPreviewAmount,
      },
      update: {
        productId: cartItem.productId,
        quantity: nextQuantity,
        isSelected: true,
        productNamePreview: cartItem.productNamePreview,
        variantNamePreview: cartItem.variantNamePreview,
        skuPreview: cartItem.skuPreview,
        imagePathPreview: cartItem.imagePathPreview,
        unitPricePreviewAmount,
        lineTotalPreviewAmount,
        sellModePreview: cartItem.sellModePreview,
        preorderReleaseDatePreview: cartItem.preorderReleaseDatePreview,
        preorderEtaNotePreview: cartItem.preorderEtaNotePreview,
      },
    });

    // Re-read the cart items to derive aggregate cart totals from persisted line data.
    const cartItems = await tx.cartItem.findMany({
      where: {
        cartId,
      },
      select: {
        quantity: true,
        lineTotalPreviewAmount: true,
      },
    });

    const itemCount = cartItems.reduce(
      (total, item) => total + item.quantity,
      0,
    );
    const subtotalAmount = cartItems.reduce(
      (total, item) => total + (item.lineTotalPreviewAmount ?? 0),
      0,
    );

    // Persist the recalculated cart summary and return the updated cart with its items.
    return tx.cart.update({
      where: {
        id: cartId,
      },
      data: {
        itemCount,
        subtotalAmount,
        lastActivityAt: new Date(),
      },
      include: {
        items: true,
      },
    });
  });
};
