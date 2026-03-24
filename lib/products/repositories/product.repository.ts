import prisma from "@/lib/infrastructure/database/prisma";
import {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";
import { AppError } from "@/core/errors/errors";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { UpdateProductFormValidationResult } from "../validation/update-product-form-data-validation";
import {
  findProductImagesToDelete,
  removeProductImagesFromStorage,
} from "./product.respository.utils";

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
  formValidation: Extract<UpdateProductFormValidationResult, { success: true }>,
) {
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

  const product = await findProductById(id, true);

  if (!product) {
    throw new AppError("PRODUCT_NOT_FOUND", "Product not found", 404);
  }

  const { productImagesToDelete, variantImagesToDelete } =
    findProductImagesToDelete(product, formValidation);

  let updatedProduct = null;
  try {
    await prisma.$transaction(async (tx) => {
      await tx.productImage.deleteMany({
        where: {
          path: {
            in: Array.from(productImagesToDelete),
          },
        },
      });
      await tx.productVariantImage.deleteMany({
        where: {
          path: {
            in: Array.from(variantImagesToDelete),
          },
        },
      });

      updatedProduct = await tx.product.update({
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
          variants: {
            deleteMany: {
              sku: {
                notIn: variants.map((variant) => variant.sku),
              },
            },
            upsert: variants.map((variant) => ({
              where: {
                sku: variant.sku,
              },
              update: {
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
              create: {
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
    });
  } catch (error) {
    throw new AppError(
      "UPDATE_PRODUCT_FAILED",
      "Failed to update product",
      500,
    );
  }

  try {
    await removeProductImagesFromStorage(
      productImagesToDelete,
      variantImagesToDelete,
    );
  } catch (error) {
    throw new AppError(
      "UPDATE_PRODUCT_FAILED",
      "Failed to update product",
      500,
    );
  }

  return updatedProduct;
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
