import { toErrorResponse } from "@/core/errors/http";
import { getCurrentCartOwner } from "@/lib/cart/http/get-current-cart-owner.http";
import { getOrCreateCurrentCartOwner } from "@/lib/cart/http/get-or-create-current-cart-owner.http";
import { addProductToCartService } from "@/lib/cart/service/add-product-to-cart.service";
import { getCartItemsService } from "@/lib/cart/service/get-cart-items.service";
import { getCartService } from "@/lib/cart/service/get-cart.service";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

/**
 * Request body contract for adding a product variant to the current cart.
 */
const addCartItemSchema = z.object({
  variantId: z.cuid("Invalid variant id"),
  quantity: z.coerce.number().int().min(1, "Quantity must be at least 1"),
});

/**
 * Returns the current cart items for the active user or guest cart owner.
 *
 * This read route does not create a guest cart token. When no cart exists yet,
 * it returns an empty collection so the client can treat the response as a
 * stable item list.
 */
export async function GET(_request: NextRequest) {
  try {
    // Resolve the current owner without creating a new guest cart for a read.
    const owner = await getCurrentCartOwner();

    if (!owner) {
      return NextResponse.json([], { status: 200 });
    }

    // Load the cart first so missing carts can still resolve to an empty item list.
    const cart = await getCartService(owner);

    if (!cart) {
      return NextResponse.json([], { status: 200 });
    }

    // Fetch and return the persisted cart items for the resolved cart id.
    const cartItems = await getCartItemsService(cart.id);

    return NextResponse.json(cartItems, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

/**
 * Adds a product variant to the current cart and returns the updated cart items.
 *
 * This mutation route is allowed to create a guest cart token because a cart
 * now needs to exist for the write to succeed.
 */
export async function POST(request: NextRequest) {
  try {
    // Parse and validate the incoming mutation payload.
    const body = await request.json();
    const { variantId, quantity } = addCartItemSchema.parse(body);

    // Mutations create a guest cart owner lazily when no owner exists yet.
    const owner = await getOrCreateCurrentCartOwner();

    // Delegate the add-or-merge behavior to the application service.
    const cart = await addProductToCartService(owner!, variantId, quantity);

    // Return the updated cart items so the client can refresh the item list directly.
    return NextResponse.json(cart.items, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
