import { MovementType } from "@/app/generated/prisma/client";
import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/infrastructure/database/prisma";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { getProductInventoryState } from "../inventory/product-inventory";
import {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";

type ProductRecord = Awaited<ReturnType<typeof prisma.product.findUnique>>;

const PRODUCT_CREATION_NOTE = "Product creation";
const PRODUCT_UPDATE_NOTE = "Product update";
const PRODUCT_TRANSFER_NOTE = "Warehouse transfer from product update";

async function ensureWarehouseExists(warehouseId: string) {
  const warehouse = await prisma.warehouse.findUnique({
    where: {
      id: warehouseId,
    },
    select: {
      id: true,
    },
  });

  if (!warehouse) {
    throw new AppError("WAREHOUSE_NOT_FOUND", "Warehouse not found", 404, {
      warehouseId,
    });
  }
}

export async function findProducts() {
  return prisma.product.findMany({
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
          stocks: {
            include: {
              warehouse: true,
            },
          },
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
    warehouseId,
    productImages,
    isPublished,
    hasVariation,
  } = command;

  await ensureWarehouseExists(warehouseId);

  return prisma.product.create({
    data: {
      name,
      description: description || undefined,
      supplierCountry,
      franchiseId,
      isPublished,
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
          stocks: {
            create: {
              warehouseId,
              quantity: variant.stock.quantity,
            },
          },
          stockMovements: {
            create: {
              warehouseId,
              type: MovementType.IN,
              quantity: variant.stock.quantity,
              note: PRODUCT_CREATION_NOTE,
              variantSku: variant.sku,
              variantName: variant.name,
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
          stocks: {
            include: {
              warehouse: true,
            },
          },
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
    warehouseId,
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

  await ensureWarehouseExists(warehouseId);
  const inventoryState = getProductInventoryState(product);

  return prisma.$transaction(async (tx) => {
    if (removedVariantIds.length > 0) {
      await tx.stock.deleteMany({
        where: {
          variantId: {
            in: removedVariantIds,
          },
          variant: {
            productId: id,
          },
        },
      });

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
      const existingVariant = variant.id
        ? product.variants.find((productVariant) => productVariant.id === variant.id)
        : undefined;
      const previousStock = existingVariant?.stocks[0];
      const previousQuantity = previousStock?.quantity ?? 0;
      const previousWarehouseId =
        previousStock?.warehouseId ?? inventoryState.warehouseId;

      let persistedVariantId: string;

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

        persistedVariantId = variant.id;
      } else {
        const createdVariant = await tx.productVariant.create({
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
          select: {
            id: true,
          },
        });

        persistedVariantId = createdVariant.id;
      }

      if (previousWarehouseId && previousWarehouseId !== warehouseId) {
        await tx.stock.deleteMany({
          where: {
            variantId: persistedVariantId,
          },
        });

        await tx.stock.upsert({
          where: {
            variantId_warehouseId: {
              variantId: persistedVariantId,
              warehouseId,
            },
          },
          create: {
            variantId: persistedVariantId,
            warehouseId,
            quantity: variant.stock.quantity,
          },
          update: {
            quantity: variant.stock.quantity,
          },
        });

        await tx.stockMovement.createMany({
          data: [
            {
              variantId: persistedVariantId,
              variantSku: variant.sku,
              variantName: variant.name,
              warehouseId: previousWarehouseId,
              type: MovementType.OUT,
              quantity: previousQuantity,
              note: PRODUCT_TRANSFER_NOTE,
            },
            {
              variantId: persistedVariantId,
              variantSku: variant.sku,
              variantName: variant.name,
              warehouseId,
              type: MovementType.IN,
              quantity: variant.stock.quantity,
              note: PRODUCT_TRANSFER_NOTE,
            },
          ],
        });

        continue;
      }

      await tx.stock.upsert({
        where: {
          variantId_warehouseId: {
            variantId: persistedVariantId,
            warehouseId,
          },
        },
        create: {
          variantId: persistedVariantId,
          warehouseId,
          quantity: variant.stock.quantity,
        },
        update: {
          quantity: variant.stock.quantity,
        },
      });

      if (!previousWarehouseId) {
        if (variant.stock.quantity !== 0) {
          await tx.stockMovement.create({
            data: {
              variantId: persistedVariantId,
              variantSku: variant.sku,
              variantName: variant.name,
              warehouseId,
              type: MovementType.IN,
              quantity: variant.stock.quantity,
              note: PRODUCT_UPDATE_NOTE,
            },
          });
        }

        continue;
      }

      const delta = variant.stock.quantity - previousQuantity;

      if (delta !== 0) {
        await tx.stockMovement.create({
          data: {
            variantId: persistedVariantId,
            variantSku: variant.sku,
            variantName: variant.name,
            warehouseId,
            type: MovementType.ADJUST,
            quantity: delta,
            note: PRODUCT_UPDATE_NOTE,
          },
        });
      }
    }

    const updatedProduct = await tx.product.findUnique({
      where: {
        id,
      },
      include: {
        variants: {
          include: {
            image: true,
            stocks: {
              include: {
                warehouse: true,
              },
            },
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
              stocks: {
                include: {
                  warehouse: true,
                },
              },
            },
          },
        }
      : {},
  });
}
