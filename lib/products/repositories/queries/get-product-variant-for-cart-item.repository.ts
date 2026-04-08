import prisma from "@/lib/infrastructure/database/prisma";

export async function getProductVariantForCartItem(variantId: string) {
  const productVariant = await prisma.productVariant.findUnique({
    where: {
      id: variantId,
    },
    include: {
      image: true,
      product: {
        select: {
          id: true,
          name: true,
          sellMode: true,
          preorderReleaseAt: true,
        },
      },
    },
  });
  return productVariant;
}
