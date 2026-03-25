import { AppError } from "@/core/errors/errors";
import {
  ProductImage,
  ProductVariantImage,
} from "@/lib/validation-schema/product-schema";
import { toUpdateProductCommand } from "../mappers/product-command.mapper";
import {
  findProductById,
  updateProduct,
} from "../repositories/product.repository";
import { validateUpdateProductFormData } from "../validation/update-product-form-data-validation";
import {
  cleanUpProductImagesService,
  uploadProductImagesService,
  uploadProductVariantImagesService,
} from "./product-image.service";
import {
  findProductImagesToDelete,
  removeProductImagesFromStorage,
} from "../repositories/product.respository.utils";

export async function updateProductService(
  productId: string,
  formData: FormData,
) {
  const product = await findProductById(productId, true);
  if (!product) {
    throw new AppError("PRODUCT_NOT_FOUND", "Product not found", 404);
  }

  const formValidation = validateUpdateProductFormData(formData);
  if (!formValidation.success) {
    throw formValidation.error;
  }
  let updatedProduct = null;

  const { productImagesToDelete, variantImagesToDelete } =
    findProductImagesToDelete(product, formValidation);

  let uploadedProductImages: ProductImage[] = [];
  let uploadedVariantImages: ProductVariantImage[] = [];

  try {
    // Try upload new images
    uploadedProductImages = await uploadProductImagesService(
      formValidation.data.newProductImages,
    );
    uploadedVariantImages = await uploadProductVariantImagesService(
      formValidation.data.variants,
      {
        mode: "update",
      },
    );

    // Turn raw formValidation data to update command form
    const command = toUpdateProductCommand(
      formValidation.data,
      uploadedProductImages,
      uploadedVariantImages,
    );

    // Update product in the database
    updatedProduct = await updateProduct(productId, command, product);
  } catch (error) {
    // Remove uploaded images if the operation failed
    await cleanUpProductImagesService(
      uploadedProductImages,
      uploadedVariantImages,
    ).catch((cleanupError) => {
      console.error("Failed to clean up uploaded product images", {
        productId,
        cleanupError,
        uploadedProductImages: uploadedProductImages.map((img) => img.path),
        uploadedVariantImages: uploadedVariantImages.map((img) => img.path),
        originalError: error,
      });
    });

    throw error;
  }

  await removeProductImagesFromStorage(
    productImagesToDelete,
    variantImagesToDelete,
  ).catch((error) => {
    console.error("Failed to clean up old product images after update", {
      productId,
      error,
      productImagesToDelete: [...productImagesToDelete],
      variantImagesToDelete: [...variantImagesToDelete],
    });
  });

  return updatedProduct;
}
