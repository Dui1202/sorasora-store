import "server-only";

import { cookies, headers } from "next/headers";
import { auth } from "@/lib/auth/auth";
import { CartOwnerContext } from "../type";
import { generateGuestCartToken } from "../generate-guest-cart-token";

export const GUEST_CART_TOKEN_COOKIE = "guest_cart_token";

export async function getOrCreateCurrentCartOwner(): Promise<CartOwnerContext> {
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
  } else {
    const newGeneratedGuestCartToken = generateGuestCartToken();
    cookieStore.set(GUEST_CART_TOKEN_COOKIE, newGeneratedGuestCartToken, {
      httpOnly: true,
      sameSite: "lax",
      secure: process.env.NODE_ENV === "production",
      path: "/",
      maxAge: 60 * 60 * 24 * 30,
    });
    return { guestCartToken: newGeneratedGuestCartToken };
  }
}
