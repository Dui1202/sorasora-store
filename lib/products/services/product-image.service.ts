import { AppError } from "@/core/errors/errors";
import {
  removeProductImages,
  uploadProductImages,
} from "@/lib/infrastructure/storage/server";
import {
  CreateProductFormValidationData,
  ProductImage,
  ProductVariantImage,
} from "@/lib/validation-schema/product-schema";

export async function uploadProductImagesService(
  data: CreateProductFormValidationData,
): Promise<ProductImage[]> {
  const uploadedProductImages: ProductImage[] = [];

  for (const image of data.productImages) {
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

export async function uploadProductVariantImagesService(
  data: CreateProductFormValidationData,
): Promise<Map<string, ProductVariantImage>> {
  const uploadedVariantImages = new Map<string, ProductVariantImage>();

  if (data.hasVariation) {
    for (const variant of data.variants) {
      if (!variant.image) {
        throw new Error("Variant image is required for every variant.");
      }

      const bytes = await variant.image.file.arrayBuffer();
      const uploadResult = await uploadProductImages(Buffer.from(bytes));

      uploadedVariantImages.set(variant.clientKey, {
        path: uploadResult.path,
        alt: variant.image.alt,
      });
    }
  }

  return uploadedVariantImages;
}

export async function cleanUpProductImagesService(
  uploadedProductImages: ProductImage[],
  uploadedVariantImages: Map<string, ProductVariantImage>,
) {
  if (uploadedProductImages.length > 0) {
    try {
      await removeProductImages([
        ...uploadedProductImages.map((img) => img.path),
        ...Array.from(uploadedVariantImages.values()).map((img) => img.path),
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
