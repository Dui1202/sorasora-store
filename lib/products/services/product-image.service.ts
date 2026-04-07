import { AppError } from "@/core/errors/errors";
import {
  removeProductImages,
  uploadProductImages,
} from "@/lib/infrastructure/storage/server";
import {
  CreateProductFormValidationData,
  LocalProductImage,
  ProductImage,
  ProductVariantImage,
  UpdateProductFormValidationData,
} from "@/lib/products/schema";

export async function uploadProductImagesService(
  productImages: LocalProductImage[],
): Promise<ProductImage[]> {
  const uploadedProductImages: ProductImage[] = [];

  for (const image of productImages) {
    const bytes = await image.file.arrayBuffer();
    const uploadResult = await uploadProductImages(Buffer.from(bytes));

    uploadedProductImages.push({
      path: uploadResult.path,
      alt: image.alt,
      position: image.position,
    });
  }

  return uploadedProductImages;
}

type CreateProductVariantImagesInput =
  CreateProductFormValidationData["variants"];
type UpdateProductVariantImagesInput =
  UpdateProductFormValidationData["variants"];

const uploadSingleProductVariantImage = async (image: {
  file: File;
  alt?: string;
}): Promise<ProductVariantImage> => {
  const bytes = await image.file.arrayBuffer();
  const uploadResult = await uploadProductImages(Buffer.from(bytes));

  return {
    path: uploadResult.path,
    alt: image.alt,
  };
};

export async function uploadProductVariantImagesService(
  variants: CreateProductVariantImagesInput,
): Promise<Map<string, ProductVariantImage>>;
export async function uploadProductVariantImagesService(
  variants: UpdateProductVariantImagesInput,
  options: {
    mode: "update";
  },
): Promise<ProductVariantImage[]>;
export async function uploadProductVariantImagesService(
  variants: CreateProductVariantImagesInput | UpdateProductVariantImagesInput,
  options?: {
    mode: "update";
  },
): Promise<Map<string, ProductVariantImage> | ProductVariantImage[]> {
  if (options?.mode === "update") {
    const uploadedVariantImages: ProductVariantImage[] = [];

    for (const variant of variants) {
      if (!("newImage" in variant) || !variant.newImage) {
        continue;
      }

      uploadedVariantImages.push(
        await uploadSingleProductVariantImage(variant.newImage),
      );
    }

    return uploadedVariantImages;
  }

  const uploadedVariantImages = new Map<string, ProductVariantImage>();

  for (const variant of variants) {
    if (!("clientKey" in variant)) {
      throw new Error("Create variant image uploads require a client key.");
    }

    if (!variant.image) {
      throw new Error("Variant image is required for every variant.");
    }

    uploadedVariantImages.set(variant.clientKey, {
      ...(await uploadSingleProductVariantImage(variant.image)),
    });
  }

  return uploadedVariantImages;
}

export async function cleanUpProductImagesService(
  uploadedProductImages: ProductImage[],
  uploadedVariantImages:
    | Map<string, ProductVariantImage>
    | ProductVariantImage[],
) {
  const variantImages = Array.isArray(uploadedVariantImages)
    ? uploadedVariantImages
    : Array.from(uploadedVariantImages.values());

  if (uploadedProductImages.length > 0 || variantImages.length > 0) {
    try {
      await removeProductImages([
        ...uploadedProductImages.map((img) => img.path),
        ...variantImages.map((img) => img.path),
      ]);
    } catch (cleanupError) {
      throw new AppError(
        "CLEANUP_PRODUCT_IMAGES_FAILED",
        "Failed to clean up product images",
        500,
        {
          cause: cleanupError,
        },
      );
    }
  }
}
