import {
  CreateProductFormData,
  ProductImage,
  ProductVariantImage,
  CreateProductCommandData,
  productBaseSchema,
} from "@/lib/validation-schema/product-schema";

export const toCreateProductCommand = (
  data: CreateProductFormData,
  uploadedProductImages: ProductImage[] = [],
  uploadedVariantImages: ProductVariantImage[] = [],
): CreateProductCommandData => {
  const productImages = productBaseSchema.shape.productImages.parse(
    uploadedProductImages,
  );
  const fallbackVariantImage = productImages[0];

  if (!fallbackVariantImage) {
    throw new Error(
      "Uploaded product images are required before creating the product command.",
    );
  }

  const variants = data.hasVariation
    ? (() => {
        if (uploadedVariantImages.length !== data.variants.length) {
          throw new Error("Variant images are required for every variant.");
        }

        return data.variants.map((variant, index) => {
          const uploadedVariantImage = uploadedVariantImages[index];

          if (!uploadedVariantImage) {
            throw new Error("Variant image is required for every variant.");
          }

          return {
            name: variant.name,
            sku: variant.sku,
            costPrice: variant.costPrice,
            sellingPrice: variant.sellingPrice,
            position: variant.position ?? index,
            image: uploadedVariantImage,
          };
        });
      })()
    : (() => {
        if (
          !data.baseSku ||
          data.baseCostPrice === undefined ||
          data.baseSellingPrice === undefined
        ) {
          throw new Error(
            "Base variant fields are required when variation is disabled.",
          );
        }

        return [
          {
            name: data.name,
            sku: data.baseSku,
            costPrice: data.baseCostPrice,
            sellingPrice: data.baseSellingPrice,
            position: 0,
            image: fallbackVariantImage,
          },
        ];
      })();

  return productBaseSchema.parse({
    name: data.name,
    description: data.description,
    supplierCountry: data.supplierCountry,
    franchiseId: data.franchiseId,
    variants,
    productImages,
    isPublished: data.isPublished,
    hasVariation: data.hasVariation,
  });
};
