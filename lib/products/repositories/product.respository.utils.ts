import { AppError } from "@/core/errors/errors";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { ProductWithRelations } from "@/lib/products/schema";
import { UpdateProductFormValidationResult } from "../validation/update-product-form-data-validation";

export const findExistingProductImages = (product: ProductWithRelations) => {
  const productImagesToDelete = new Set(
    product.productImages.map((image) => image.path),
  );
  const variantImagesToDelete = new Set(
    product.variants.flatMap((variant) =>
      variant.image ? [variant.image.path] : [],
    ),
  );

  // Single-variant products can reuse the first product image as the variant image.
  for (const productImagePath of productImagesToDelete) {
    variantImagesToDelete.delete(productImagePath);
  }

  return {
    productImagesToDelete,
    variantImagesToDelete,
  };
};

export const findProductImagesToDelete = (
  product: ProductWithRelations,
  formValidation: Extract<UpdateProductFormValidationResult, { success: true }>,
) => {
  const { productImagesToDelete, variantImagesToDelete } =
    findExistingProductImages(product);

  for (const image of formValidation.data.existProductImages) {
    productImagesToDelete.delete(image.path);
  }

  for (const variant of formValidation.data.variants) {
    if (variant.existImage) {
      variantImagesToDelete.delete(variant.existImage.path);
    }
  }

  return { productImagesToDelete, variantImagesToDelete };
};

export const removeProductImagesFromStorage = async (
  productImagesToDelete: Set<string>,
  variantImagesToDelete: Set<string>,
) => {
  if (productImagesToDelete.size === 0 && variantImagesToDelete.size === 0) {
    return;
  }

  try {
    const removals: Promise<unknown>[] = [];

    if (productImagesToDelete.size > 0) {
      removals.push(removeProductImages([...productImagesToDelete]));
    }

    if (variantImagesToDelete.size > 0) {
      removals.push(removeProductImages([...variantImagesToDelete]));
    }

    await Promise.all(removals);
  } catch {
    throw new AppError(
      "REMOVE_IMAGE_FAILED",
      "Failed to remove image from storage",
      500,
    );
  }
};
