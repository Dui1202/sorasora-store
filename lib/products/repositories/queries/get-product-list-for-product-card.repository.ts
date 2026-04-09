import prisma from "@/lib/infrastructure/database/prisma";

/**
 * Returns the published product list shape required by the storefront product cards.
 *
 * Scalar fields are selected directly, while relations use nested selects so the
 * payload stays explicit and minimal for the store landing page.
 */
export async function getProductListForProductCard() {
  return prisma.product.findMany({
    where: {
      isArchived: false,
      isPublished: true,
    },
    select: {
      id: true,
      name: true,
      sellMode: true,
      preorderReleaseAt: true,
      productImages: {
        orderBy: {
          position: "asc",
        },
        select: {
          path: true,
        },
      },
      variants: {
        where: {
          isArchived: false,
        },
        orderBy: {
          position: "asc",
        },
        select: {
          sellingPrice: true,
        },
      },
    },
  });
}
