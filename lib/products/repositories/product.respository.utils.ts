import { AppError } from "@/core/errors/errors";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { UpdateProductFormValidationResult } from "../validation/update-product-form-data-validation";

export const findProductImagesToDelete = (
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

  return { productImagesToDelete, variantImagesToDelete };
};

export const removeProductImagesFromStorage = async (
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
