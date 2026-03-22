import z from "zod";
import {
  productBaseSchema,
  ProductImages,
  ProductVariantImages,
  ProductWithRelations,
} from "./product-base";
import { CreateProductFormData } from "./create-product-form";
import { UpdateProductFormInputData } from "./update-product-form";

// Explicit mapper
export const toCreateProductCommand = (
  data: CreateProductFormData,
  uploadedProductImages: ProductImages = [],
  uploadedVariantImages: ProductVariantImages = [],
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
    variants: variants.map((variant) => ({
      name: variant.name,
      sku: variant.sku,
      costPrice: variant.costPrice,
      sellingPrice: variant.sellingPrice,
      position: variant.position,
      image: variant.image
        ? { path: variant.image.path, alt: variant.image.alt ?? undefined }
        : undefined,
    })),
    productImages: [...product.productImages]
      .sort((a, b) => a.position - b.position)
      .map((img) => ({
        path: img.path,
        alt: img.alt ?? "",
        position: img.position,
      })),
  };
};

export type CreateProductCommandData = z.output<typeof productBaseSchema>;
