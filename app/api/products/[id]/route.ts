import { Prisma } from "@/app/generated/prisma/client";
import { AppError } from "@/core/errors/errors";
import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import {
  removeProductImages,
  uploadProductImages,
} from "@/lib/infrastructure/storage/server";
import {
  UpdateProductFormValidationResult,
  validateUpdateProductFormData,
} from "@/lib/products/validation";
import {
  ProductImages,
  ProductVariantImages,
  ProductWithRelations,
} from "@/lib/validation-schema/product-schema";
import {
  toUpdateProductCommand,
  UpdateProductCommandData,
} from "@/lib/validation-schema/product-schema/product-command";
import { NextRequest, NextResponse } from "next/server";

type Params = {
  params: Promise<{ id: string }>;
};

export async function DELETE(_request: NextRequest, { params }: Params) {
  const { id } = await params;
  const product = await prisma.product.findUnique({
    where: {
      id: id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  await removeProductImages(product.productImages.map((image) => image.path));

  if (product.variants.length > 1 && product.variants[0].image) {
    await removeProductImages(
      product.variants.map((variant) => variant.image!.path),
    );
  }

  const deleteProduct = await prisma.product.delete({
    where: {
      id: id,
    },
  });

  return NextResponse.json(deleteProduct, { status: 200 });
}

export async function PATCH(request: NextRequest, { params }: Params) {
  const { id } = await params;

  let formData: FormData;

  //  Default error
  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data" },
      { status: 400 },
    );
  }

  const product = await prisma.product.findUnique({
    where: {
      id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  const formValidation = validateUpdateProductFormData(formData);

  if (!formValidation.success) {
    return NextResponse.json({ error: formValidation.error }, { status: 400 });
  }

  try {
    await removeExistingImages(product, formValidation);
  } catch (error) {
    return toErrorResponse(error);
  }

  const uploadedProductImages: ProductImages = [];
  const uploadedVariantImages: ProductVariantImages = [];

  try {
    for (const image of formValidation.data.newProductImages) {
      const bytes = await image.file.arrayBuffer();
      const uploadResult = await uploadProductImages(Buffer.from(bytes));

      uploadedProductImages.push({
        path: uploadResult.path,
        alt: image.alt,
        position: image.position,
      });
    }

    if (formValidation.data.hasVariation) {
      for (const variant of formValidation.data.variants) {
        if (!variant.newImage) {
          continue;
        }

        const bytes = await variant.newImage.file.arrayBuffer();
        const uploadResult = await uploadProductImages(Buffer.from(bytes));

        uploadedVariantImages.push({
          path: uploadResult.path,
          alt: variant.newImage.alt,
        });
      }
    }
    const command = toUpdateProductCommand(
      formValidation.data,
      uploadedProductImages,
      uploadedVariantImages,
    );
    const updatedProduct = await updateProduct(id, command);

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    if (uploadedProductImages.length > 0 || uploadedVariantImages.length > 0) {
      try {
        await removeProductImages([
          ...uploadedProductImages.map((image) => image.path),
          ...uploadedVariantImages.map((image) => image.path),
        ]);
      } catch (cleanupError) {
        console.error(
          "Failed to clean up uploaded images after update failure",
          cleanupError,
        );
      }
    }

    return toErrorResponse(error);
  }
}

const updateProduct = async (id: string, command: UpdateProductCommandData) => {
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
};

const removeExistingImages = async (
  product: ProductWithRelations,
  formValidation: Extract<UpdateProductFormValidationResult, { success: true }>,
) => {
  let productImagesToDelete: Set<string> = new Set(
    product.productImages.map((image) => image.path),
  );

  let variantImagesToDelete: Set<string> = new Set();

  if (product.hasVariation) {
    variantImagesToDelete = new Set(
      product.variants.map((variant) => variant.image!.path),
    );
  }

  for (const image of formValidation.data.existProductImages) {
    productImagesToDelete.delete(image.path);
  }

  for (const variant of formValidation.data.variants) {
    if (variant.existImage) {
      variantImagesToDelete.delete(variant.existImage.path);
    }
  }

  await removeProductImagesRecords(
    productImagesToDelete,
    variantImagesToDelete,
  );

  await removeProductImagesFromStorage(
    productImagesToDelete,
    variantImagesToDelete,
  );
};

const removeProductImagesRecords = async (
  productImagesToDelete: Set<string>,
  variantImagesToDelete: Set<string>,
) => {
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
    });
  } catch (err) {
    throw new AppError(
      "REMOVE_IMAGE_FAILED",
      "Failed to remove image from database",
      500,
    );
  }
};
const removeProductImagesFromStorage = async (
  productImagesToDelete: Set<string>,
  variantImagesToDelete: Set<string>,
) => {
  try {
    await Promise.all([
      removeProductImages([...productImagesToDelete]),
      removeProductImages([...variantImagesToDelete]),
    ]);
  } catch (err) {
    throw new AppError(
      "REMOVE_IMAGE_FAILED",
      "Failed to remove image from storage",
      500,
    );
  }
};

export async function GET(_request: NextRequest, { params }: Params) {
  const { id } = await params;

  const product = await prisma.product.findUnique({
    where: {
      id: id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  return NextResponse.json(product, { status: 200 });
}
