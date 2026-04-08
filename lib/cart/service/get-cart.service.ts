import { AppError } from "@/core/errors/errors";
import App from "next/app";
import { getGuestCart, getUserCart } from "../repositories/get-cart.repository";
import { CartOwnerContext } from "../type";

export async function getCartService(owner: CartOwnerContext) {
  if (!owner || (!owner.userId && !owner.guestCartToken)) {
    throw new AppError(
      "NO_OWNER_CONTEXT_FOUND",
      "Either userId or guestCartToken must be provided to retrieve the cart.",
      404,
    );
  }

  let cart;

  if (owner.userId) {
    cart = await getUserCart(owner.userId);
  } else if (owner.guestCartToken) {
    cart = await getGuestCart(owner.guestCartToken);
  }

  return cart;
}
