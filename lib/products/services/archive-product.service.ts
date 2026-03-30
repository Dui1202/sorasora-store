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

  await removeProductImagesFromStorage(
    productImagesToDelete,
    variantImagesToDelete,
  ).catch((error) => {
    console.error(
      "Error removing product images from storage after archiving product:",
      error,
    );
  });

  return archivedProduct;
}
