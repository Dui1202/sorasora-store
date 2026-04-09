import prisma from "@/lib/infrastructure/database/prisma";

export async function getUserCart(userId: string) {
  const cart = await prisma.cart.findUnique({
    where: {
      userId,
    },
    include: {
      items: true,
    },
  });
  return cart;
}

export async function getGuestCart(guestCartToken: string) {
  const cart = await prisma.cart.findUnique({
    where: {
      guestCartToken,
    },
    include: {
      items: true,
    },
  });
  return cart;
}
