import prisma from "@/lib/infrastructure/database/prisma";

export async function getUserCart(userId: string) {
  const cart = await prisma.cart.findUnique({
    where: {
      userId,
    },
  });
  return cart;
}

export async function getGuestCart(guestCartToken: string) {
  const cart = await prisma.cart.findUnique({
    where: {
      guestCartToken,
    },
  });
  return cart;
}
