import prisma from "@/lib/infrastructure/database/prisma";
import {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";

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

  return prisma.product.update({
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
}
