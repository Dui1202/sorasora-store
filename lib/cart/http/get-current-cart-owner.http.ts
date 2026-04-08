import "server-only";

import { cookies, headers } from "next/headers";
import { auth } from "@/lib/auth/auth";
import { CartOwnerContext } from "../type";
import { GUEST_CART_TOKEN_COOKIE } from "./get-or-create-current-cart-owner.http";

export async function getCurrentCartOwner(): Promise<CartOwnerContext> {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (session?.user.id) {
    return { userId: session.user.id };
  }

  const cookieStore = await cookies();
  const guestCartToken = cookieStore.get(GUEST_CART_TOKEN_COOKIE)?.value;

  if (guestCartToken) {
    return { guestCartToken };
  }

  return null;
}
