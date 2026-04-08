import prisma from "@/lib/infrastructure/database/prisma";

export async function getCartItemsService(cartId: string) {
  const cartItems = await prisma.cartItem.findMany({
    where: {
      cartId,
    },
  });
  return cartItems;
}
