import z from "zod";
import {
  productBaseSchema,
  ProductImage,
  ProductVariantImage,
  ProductWithRelations,
} from "./product-base";
import {
  UpdateProductFormData,
  UpdateProductFormInputData,
} from "./update-product-form";

// Explicit mapper

export const toUpdateProductCommand = (
  data: UpdateProductFormData,
  uploadedProductImages: ProductImage[] = [],
  uploadedVariantImages: ProductVariantImage[] = [],
): UpdateProductCommandData => {
  if (uploadedProductImages.length !== data.newProductImages.length) {
    throw new Error(
      "Uploaded product images count does not match the new product images.",
    );
  }

  const expectedUploadedVariantImages = data.hasVariation
    ? data.variants.filter((variant) => variant.newImage).length
    : 0;

  if (uploadedVariantImages.length !== expectedUploadedVariantImages) {
    throw new Error(
      "Uploaded variant images count does not match variants requiring new images.",
    );
  }

  const productImages = productBaseSchema.shape.productImages
    .parse([...data.existProductImages, ...uploadedProductImages])
    .sort((a, b) => a.position - b.position);
  const fallbackVariantImage = productImages[0];

  if (!fallbackVariantImage) {
    throw new Error(
      "At least one product image is required before updating the product command.",
    );
  }

  let uploadedVariantImageIndex = 0;

  const variants = data.hasVariation
    ? data.variants.map((variant, index) => {
        const image =
          variant.existImage ??
          (() => {
            const uploadedVariantImage =
              uploadedVariantImages[uploadedVariantImageIndex];

            if (!uploadedVariantImage) {
              throw new Error("Variant image is required for every variant.");
            }

            uploadedVariantImageIndex += 1;

            return uploadedVariantImage;
          })();

        return {
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
          position: variant.position ?? index,
          image,
        };
      })
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

export const toUpdateProductCommandInput = (
  product: ProductWithRelations,
): UpdateProductFormInputData => {
  const variants = [...product.variants].sort(
    (a, b) => a.position - b.position,
  );
  const hasVariation = product.hasVariation;
  const baseVariant = variants[0];

  return {
    name: product.name,
    description: product.description ?? undefined,
    supplierCountry: product.supplierCountry,
    franchiseId: product.franchiseId ?? undefined,
    hasVariation,
    baseSku: hasVariation ? undefined : baseVariant.sku,
    baseCostPrice: hasVariation ? undefined : baseVariant.costPrice,
    baseSellingPrice: hasVariation ? undefined : baseVariant.sellingPrice,
    variants: hasVariation
      ? undefined
      : variants.map((variant) => ({
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
          position: variant.position,
          newImage: undefined,
          existImage: {
            path: variant.image!.path,
            alt: variant.image!.alt ?? undefined,
          },
        })),
    newProductImages: [],
    existProductImages: product.productImages.map((image) => ({
      path: image.path,
      alt: image.alt ?? undefined,
    })),
  };
};

export type CreateProductCommandData = z.output<typeof productBaseSchema>;
export type UpdateProductCommandData = z.output<typeof productBaseSchema>;
