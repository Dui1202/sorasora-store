import {
  ProductImage,
  ProductVariantImage,
} from "@/lib/validation-schema/product-schema";
import { toCreateProductCommand } from "../mappers/product-command.mapper";
import { createProduct } from "../repositories/product.repository";
import { validateCreateProductFormData } from "../validation/create-product-form-data-validation";
import {
  cleanUpProductImagesService,
  uploadProductImagesService,
  uploadProductVariantImagesService,
} from "./product-image.service";

export async function createProductService(formData: FormData) {
  const formValidation = validateCreateProductFormData(formData);
  if (!formValidation.success) {
    throw formValidation.error;
  }

  let uploadedProductImages: ProductImage[] = [];
  let uploadedVariantImages: ProductVariantImage[] = [];

  try {
    uploadedProductImages = await uploadProductImagesService(
      formValidation.data,
    );
    uploadedVariantImages = await uploadProductVariantImagesService(
      formValidation.data,
    );

    const command = toCreateProductCommand(
      formValidation.data,
      uploadedProductImages,
      uploadedVariantImages,
    );
    const newProduct = await createProduct(command);

    return newProduct;
  } catch (error) {
    await cleanUpProductImagesService(
      uploadedProductImages,
      uploadedVariantImages,
    );
    throw error;
  }
}
