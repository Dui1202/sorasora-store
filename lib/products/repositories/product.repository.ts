import { MovementType } from "@/app/generated/prisma/client";
import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/infrastructure/database/prisma";
import { ProductWithRelations } from "@/lib/products/schema";
import { getProductInventoryState } from "../inventory/product-inventory";
import {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";

type ProductRecord = Awaited<ReturnType<typeof prisma.product.findFirst>>;
type PrismaTransaction = Parameters<
  Parameters<typeof prisma.$transaction>[0]
>[0];

const PRODUCT_CREATION_NOTE = "Product creation";
const PRODUCT_UPDATE_NOTE = "Product update";
const PRODUCT_TRANSFER_NOTE = "Warehouse transfer from product update";

const activeProductRelations = {
  productImages: true,
  variations: {
    orderBy: {
      position: "asc" as const,
    },
    include: {
      options: {
        orderBy: {
          position: "asc" as const,
        },
      },
    },
  },
  variants: {
    where: {
      isArchived: false,
    },
    orderBy: {
      position: "asc" as const,
    },
    include: {
      image: true,
      variationOptions: {
        include: {
          variation: true,
          variationOption: true,
        },
      },
      stocks: {
        include: {
          warehouse: true,
        },
      },
    },
  },
};

function getSelectionKey(variationId: string, optionId: string) {
  return `${variationId}:${optionId}`;
}

async function ensureWarehouseExists(warehouseId: string): Promise<void> {
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

async function getProductWithRelationsOrThrow(
  db: PrismaTransaction,
  productId: string,
): Promise<ProductWithRelations> {
  const product = await db.product.findFirst({
    where: {
      id: productId,
      isArchived: false,
    },
    include: activeProductRelations,
  });

  if (!product) {
    throw new AppError("PRODUCT_NOT_FOUND", "Product not found", 404);
  }

  return product;
}

async function createVariationStructure(
  db: PrismaTransaction,
  productId: string,
  variations:
    | CreateProductCommandData["variations"]
    | UpdateProductCommandData["variations"],
) {
  const persistedVariationIdByInputId = new Map<string, string>();
  const persistedOptionIdByInputKey = new Map<string, string>();

  for (const variation of variations) {
    const createdVariation = await db.variation.create({
      data: {
        productId,
        name: variation.name,
        position: variation.position,
        options: {
          create: variation.options.map((option) => ({
            value: option.value,
            position: option.position,
          })),
        },
      },
      include: {
        options: true,
      },
    });

    persistedVariationIdByInputId.set(variation.id, createdVariation.id);

    for (const option of variation.options) {
      const persistedOption = createdVariation.options.find((createdOption) => {
        return (
          createdOption.value === option.value &&
          createdOption.position === option.position
        );
      });

      if (!persistedOption) {
        throw new AppError(
          "UPDATE_PRODUCT_FAILED",
          "Variation option could not be persisted",
          500,
        );
      }

      persistedOptionIdByInputKey.set(
        getSelectionKey(variation.id, option.id),
        persistedOption.id,
      );
    }
  }

  return {
    persistedVariationIdByInputId,
    persistedOptionIdByInputKey,
  };
}

async function createVariantSelectionLinks(
  db: PrismaTransaction,
  persistedVariantId: string,
  selectedOptions: Array<{
    variationId: string;
    optionId: string;
  }>,
  variationStructure: Awaited<ReturnType<typeof createVariationStructure>>,
) {
  if (selectedOptions.length === 0) {
    return;
  }

  await db.productVariantOption.createMany({
    data: selectedOptions.map((selection) => {
      const persistedVariationId =
        variationStructure.persistedVariationIdByInputId.get(
          selection.variationId,
        );
      const persistedOptionId =
        variationStructure.persistedOptionIdByInputKey.get(
          getSelectionKey(selection.variationId, selection.optionId),
        );

      if (!persistedVariationId || !persistedOptionId) {
        throw new AppError(
          "UPDATE_PRODUCT_FAILED",
          "Variant selection does not match persisted variations",
          400,
        );
      }

      return {
        productVariantId: persistedVariantId,
        variationId: persistedVariationId,
        variationOptionId: persistedOptionId,
      };
    }),
  });
}

async function persistVariant(
  db: PrismaTransaction,
  productId: string,
  warehouseId: string,
  variant:
    | CreateProductCommandData["variants"][number]
    | UpdateProductCommandData["variants"][number],
  previousWarehouseId: string | undefined,
  previousQuantity: number,
  movementNoteWhenNoPreviousStock = PRODUCT_UPDATE_NOTE,
) {
  let persistedVariantId: string;

  if ("id" in variant && variant.id) {
    await db.productVariant.update({
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
    const createdVariant = await db.productVariant.create({
      data: {
        productId,
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
    await db.stock.deleteMany({
      where: {
        variantId: persistedVariantId,
      },
    });

    await db.stock.upsert({
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

    await db.stockMovement.createMany({
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

    return persistedVariantId;
  }

  await db.stock.upsert({
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
      await db.stockMovement.create({
        data: {
          variantId: persistedVariantId,
          variantSku: variant.sku,
          variantName: variant.name,
          warehouseId,
          type: MovementType.IN,
          quantity: variant.stock.quantity,
          note: movementNoteWhenNoPreviousStock,
        },
      });
    }

    return persistedVariantId;
  }

  const delta = variant.stock.quantity - previousQuantity;

  if (delta !== 0) {
    await db.stockMovement.create({
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

  return persistedVariantId;
}

export async function findProducts(): Promise<ProductWithRelations[]> {
  return prisma.product.findMany({
    where: {
      isArchived: false,
    },
    include: activeProductRelations,
  });
}

export async function createProduct(
  command: CreateProductCommandData,
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
    variations,
    variants,
  } = command;

  await ensureWarehouseExists(warehouseId);

  return prisma.$transaction(async (tx) => {
    const createdProduct = await tx.product.create({
      data: {
        name,
        description: description || undefined,
        supplierCountry,
        franchiseId,
        isPublished,
        hasVariation,
        productImages: {
          create: productImages.map((productImage) => ({
            path: productImage.path,
            alt: productImage.alt,
            position: productImage.position,
          })),
        },
      },
      select: {
        id: true,
      },
    });

    const variationStructure = hasVariation
      ? await createVariationStructure(tx, createdProduct.id, variations)
      : null;

    for (const variant of variants) {
      const persistedVariantId = await persistVariant(
        tx,
        createdProduct.id,
        warehouseId,
        variant,
        undefined,
        0,
        PRODUCT_CREATION_NOTE,
      );

      if (variationStructure) {
        await createVariantSelectionLinks(
          tx,
          persistedVariantId,
          variant.selectedOptions,
          variationStructure,
        );
      }
    }

    return getProductWithRelationsOrThrow(tx, createdProduct.id);
  });
}

export async function archiveProduct(id: string) {
  return prisma.$transaction(async (tx) => {
    const activeVariantIds = (
      await tx.productVariant.findMany({
        where: {
          productId: id,
          isArchived: false,
        },
        select: {
          id: true,
        },
      })
    ).map((variant) => variant.id);

    if (activeVariantIds.length > 0) {
      await tx.stock.deleteMany({
        where: {
          variantId: {
            in: activeVariantIds,
          },
        },
      });

      await tx.productVariantImage.deleteMany({
        where: {
          variantId: {
            in: activeVariantIds,
          },
        },
      });

      await tx.productVariant.updateMany({
        where: {
          productId: id,
          id: {
            in: activeVariantIds,
          },
        },
        data: {
          isArchived: true,
        },
      });
    }

    await tx.productImage.deleteMany({
      where: {
        productId: id,
      },
    });

    return tx.product.update({
      where: {
        id,
      },
      data: {
        isArchived: true,
        isPublished: false,
      },
    });
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
    variations,
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

      await tx.productVariantOption.deleteMany({
        where: {
          productVariantId: {
            in: removedVariantIds,
          },
        },
      });

      await tx.productVariant.updateMany({
        where: {
          productId: id,
          id: {
            in: removedVariantIds,
          },
        },
        data: {
          isArchived: true,
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

    await tx.variation.deleteMany({
      where: {
        productId: id,
      },
    });

    const variationStructure = hasVariation
      ? await createVariationStructure(tx, id, variations)
      : null;

    for (const variant of variants) {
      const existingVariant = variant.id
        ? product.variants.find(
            (productVariant) => productVariant.id === variant.id,
          )
        : undefined;
      const previousStock = existingVariant?.stocks[0];
      const previousQuantity = previousStock?.quantity ?? 0;
      const previousWarehouseId =
        previousStock?.warehouseId ?? inventoryState.warehouseId;

      const persistedVariantId = await persistVariant(
        tx,
        id,
        warehouseId,
        variant,
        previousWarehouseId,
        previousQuantity,
      );

      if (variationStructure) {
        await createVariantSelectionLinks(
          tx,
          persistedVariantId,
          variant.selectedOptions,
          variationStructure,
        );
      }
    }

    return getProductWithRelationsOrThrow(tx, id);
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
  return await prisma.product.findFirst({
    where: {
      id,
      isArchived: false,
    },
    include: hasRelations ? activeProductRelations : undefined,
  });
}

export async function updateProductPublishState(
  id: string,
  isPublished: boolean,
) {
  return await prisma.product.update({
    where: {
      id,
    },
    data: {
      isPublished,
    },
  });
}
