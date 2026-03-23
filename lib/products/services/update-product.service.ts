import {
  ProductImage,
  ProductVariantImage,
} from "@/lib/validation-schema/product-schema";
import { toUpdateProductCommand } from "../mappers/product-command.mapper";
import { updateProduct } from "../repositories/product.repository";
import { validateUpdateProductFormData } from "../validation/update-product-form-data-validation";
import {
  cleanUpProductImagesService,
  uploadProductImagesService,
  uploadProductVariantImagesService,
} from "./product-image.service";

export async function updateProductService(
  productId: string,
  formData: FormData,
) {
  const formValidation = validateUpdateProductFormData(formData);
  if (!formValidation.success) {
    throw formValidation.error;
  }

  let uploadedProductImages: ProductImage[] = [];
  let uploadedVariantImages: ProductVariantImage[] = [];

  try {
    uploadedProductImages = await uploadProductImagesService(
      formValidation.data.newProductImages,
    );
    uploadedVariantImages = await uploadProductVariantImagesService(
      formValidation.data.variants,
      {
        mode: "update",
      },
    );

    const command = toUpdateProductCommand(
      formValidation.data,
      uploadedProductImages,
      uploadedVariantImages,
    );
    const updatedProduct = await updateProduct(productId, command);

    return updatedProduct;
  } catch (error) {
    await cleanUpProductImagesService(
      uploadedProductImages,
      uploadedVariantImages,
    );
    throw error;
  }
}
