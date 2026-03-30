import { AppError } from "@/core/errors/errors";
import {
  archiveProduct,
  findProductById,
} from "../repositories/product.repository";
import {
  findExistingProductImages,
  removeProductImagesFromStorage,
} from "../repositories/product.respository.utils";

export async function archiveProductService(productId: string) {
  const product = await findProductById(productId, true);

  if (!product) {
    throw new AppError("PRODUCT_NOT_FOUND", "Product not found", 404);
  }

  const { productImagesToDelete, variantImagesToDelete } =
    findExistingProductImages(product);

  const archivedProduct = await archiveProduct(productId);

  try {
    await removeProductImagesFromStorage(
      productImagesToDelete,
      variantImagesToDelete,
    );
  } catch (error) {
    throw new AppError(
      "ARCHIVE_PRODUCT_IMAGE_CLEANUP_FAILED",
      "Product archived but image cleanup failed",
      500,
      {
        productId,
        cause: error,
        productImagesToDelete: [...productImagesToDelete],
        variantImagesToDelete: [...variantImagesToDelete],
      },
    );
  }

  return archivedProduct;
}
