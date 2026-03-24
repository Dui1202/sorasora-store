import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { updateProductService } from "@/lib/products/services/update-product.service";

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

  try {
    const updatedProduct = await updateProductService(id, formData);

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

// const removeExistingImages = async (
//   product: ProductWithRelations,
//   formValidation: Extract<UpdateProductFormValidationResult, { success: true }>,
// ) => {
//   let productImagesToDelete: Set<string> = new Set(
//     product.productImages.map((image) => image.path),
//   );

//   let variantImagesToDelete: Set<string> = new Set();

//   if (product.hasVariation) {
//     variantImagesToDelete = new Set(
//       product.variants.map((variant) => variant.image!.path),
//     );
//   }

//   for (const image of formValidation.data.existProductImages) {
//     productImagesToDelete.delete(image.path);
//   }

//   for (const variant of formValidation.data.variants) {
//     if (variant.existImage) {
//       variantImagesToDelete.delete(variant.existImage.path);
//     }
//   }

//   await removeProductImagesRecords(
//     productImagesToDelete,
//     variantImagesToDelete,
//   );

//   await removeProductImagesFromStorage(
//     productImagesToDelete,
//     variantImagesToDelete,
//   );
// };

// const removeProductImagesRecords = async (
//   productImagesToDelete: Set<string>,
//   variantImagesToDelete: Set<string>,
// ) => {
//   try {
//     await prisma.$transaction(async (tx) => {
//       await tx.productImage.deleteMany({
//         where: {
//           path: {
//             in: Array.from(productImagesToDelete),
//           },
//         },
//       });
//       await tx.productVariantImage.deleteMany({
//         where: {
//           path: {
//             in: Array.from(variantImagesToDelete),
//           },
//         },
//       });
//     });
//   } catch (err) {
//     throw new AppError(
//       "REMOVE_IMAGE_FAILED",
//       "Failed to remove image from database",
//       500,
//     );
//   }
// };
// const removeProductImagesFromStorage = async (
//   productImagesToDelete: Set<string>,
//   variantImagesToDelete: Set<string>,
// ) => {
//   try {
//     await Promise.all([
//       removeProductImages([...productImagesToDelete]),
//       removeProductImages([...variantImagesToDelete]),
//     ]);
//   } catch (err) {
//     throw new AppError(
//       "REMOVE_IMAGE_FAILED",
//       "Failed to remove image from storage",
//       500,
//     );
//   }
// };

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
