import prisma from "@/lib/infrastructure/database/prisma";
import {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";
import { AppError } from "@/core/errors/errors";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";

type ProductRecord = Awaited<ReturnType<typeof prisma.product.findUnique>>;

export async function findProducts() {
  return prisma.product.findMany({
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });
}

export async function createProduct(command: CreateProductCommandData) {
  const {
    name,
    description,
    variants,
    supplierCountry,
    franchiseId,
    productImages,
    isPublished,
    hasVariation,
  } = command;

  return prisma.product.create({
    data: {
      name,
      description: description || undefined,
      supplierCountry,
      franchiseId,
      isPublished: isPublished,
      hasVariation,
      variants: {
        create: variants.map((variant) => ({
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
          position: variant.position,
          image: {
            create: {
              path: variant.image.path,
              alt: variant.image.alt,
            },
          },
        })),
      },

      productImages: {
        create: productImages.map((productImage) => ({
          path: productImage.path,
          alt: productImage.alt,
          position: productImage.position,
        })),
      },
    },
    include: {
      variants: {
        include: {
          image: true,
        },
      },
      productImages: true,
    },
  });
}

export async function updateProduct(
  id: string,
  command: UpdateProductCommandData,
  product: ProductWithRelations,
): Promise<ProductWithRelations> {
  const {
    name,
    description,
    supplierCountry,
    franchiseId,
    isPublished,
    hasVariation,
    productImages,
    variants,
  } = command;

  const existingVariantIds = new Set(
    product.variants.map((variant) => variant.id),
  );
  const submittedExistingVariantIds = variants.flatMap((variant) =>
    variant.id ? [variant.id] : [],
  );
  const removedVariantIds = product.variants
    .filter((variant) => !submittedExistingVariantIds.includes(variant.id))
    .map((variant) => variant.id);

  for (const variant of variants) {
    if (variant.id && !existingVariantIds.has(variant.id)) {
      throw new AppError(
        "UPDATE_PRODUCT_FAILED",
        "Variant does not belong to product",
        400,
      );
    }
  }

  return prisma.$transaction(async (tx) => {
    if (removedVariantIds.length > 0) {
      await tx.productVariantImage.deleteMany({
        where: {
          variantId: {
            in: removedVariantIds,
          },
          variant: {
            productId: id,
          },
        },
      });

      await tx.productVariant.deleteMany({
        where: {
          productId: id,
          id: {
            in: removedVariantIds,
          },
        },
      });
    }

    await tx.product.update({
      where: {
        id,
      },
      data: {
        name,
        description: description || null,
        supplierCountry,
        franchiseId: franchiseId ?? null,
        isPublished,
        hasVariation,
        productImages: {
          deleteMany: {},
          create: productImages.map((productImage) => ({
            path: productImage.path,
            alt: productImage.alt,
            position: productImage.position,
          })),
        },
      },
    });

    for (const variant of variants) {
      if (variant.id) {
        await tx.productVariant.update({
          where: {
            id: variant.id,
          },
          data: {
            name: variant.name,
            sku: variant.sku,
            costPrice: variant.costPrice,
            sellingPrice: variant.sellingPrice,
            position: variant.position,
            image: {
              upsert: {
                create: {
                  path: variant.image.path,
                  alt: variant.image.alt,
                },
                update: {
                  path: variant.image.path,
                  alt: variant.image.alt,
                },
              },
            },
          },
        });

        continue;
      }

      await tx.productVariant.create({
        data: {
          productId: id,
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
          position: variant.position,
          image: {
            create: {
              path: variant.image.path,
              alt: variant.image.alt,
            },
          },
        },
      });
    }

    const updatedProduct = await tx.product.findUnique({
      where: {
        id,
      },
      include: {
        variants: {
          include: {
            image: true,
          },
        },
        productImages: true,
      },
    });

    if (!updatedProduct) {
      throw new AppError("PRODUCT_NOT_FOUND", "Product not found", 404);
    }

    return updatedProduct;
  });
}
export async function findProductById(
  id: string,
  hasRelations: true,
): Promise<ProductWithRelations | null>;
export async function findProductById(
  id: string,
  hasRelations: false,
): Promise<ProductRecord>;
export async function findProductById(
  id: string,
  hasRelations: boolean,
): Promise<ProductWithRelations | ProductRecord>;
export async function findProductById(
  id: string,
  hasRelations: boolean,
): Promise<ProductWithRelations | ProductRecord> {
  return await prisma.product.findUnique({
    where: {
      id,
    },
    include: hasRelations
      ? {
          productImages: true,
          variants: {
            include: {
              image: true,
            },
          },
        }
      : {},
  });
}
