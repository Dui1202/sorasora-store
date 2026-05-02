import {
  CreateProductFormData,
  ProductImage,
  ProductVariation,
  ProductVariantImage,
  productBaseSchema,
  productVariantBaseSchema,
  ProductWithRelations,
  UpdateProductFormData,
  UpdateProductFormInputData,
} from "@/lib/products/schema";
import { buildVariationDisplayName } from "@/lib/products/schema/variation-contract";
import { getProductInventoryState } from "../inventory/product-inventory";
import z from "zod";

const updateProductCommandSchema = productBaseSchema.extend({
  variants: z.array(
    productVariantBaseSchema.extend({
      id: z.cuid("Invalid variant id").optional(),
    }),
  ),
});

type VariationAwareFormData = Pick<
  CreateProductFormData | UpdateProductFormData,
  "hasVariation" | "variations"
>;

type VariationAwareVariant = {
  selectedOptions: Array<{
    variationId: string;
    optionId: string;
  }>;
};

function sortVariations(variations: ProductVariation[]) {
  return [...variations]
    .sort((a, b) => a.position - b.position)
    .map((variation) => ({
      ...variation,
      options: [...variation.options].sort((a, b) => a.position - b.position),
    }));
}

function sortSelectedOptions(
  variations: ProductVariation[],
  selectedOptions: VariationAwareVariant["selectedOptions"],
) {
  const positionsByVariationId = new Map(
    variations.map((variation) => [variation.id, variation.position]),
  );

  return [...selectedOptions].sort((left, right) => {
    return (
      (positionsByVariationId.get(left.variationId) ?? 0) -
      (positionsByVariationId.get(right.variationId) ?? 0)
    );
  });
}

function getCommandVariations(data: VariationAwareFormData) {
  if (!data.hasVariation) {
    return [] as ProductVariation[];
  }

  return sortVariations(data.variations);
}

function buildVariationVariantName(
  variations: ProductVariation[],
  selectedOptions: VariationAwareVariant["selectedOptions"],
) {
  return buildVariationDisplayName(
    variations,
    sortSelectedOptions(variations, selectedOptions),
  );
}

function mapCreateVariationVariants(
  data: CreateProductFormData,
  variations: ProductVariation[],
  uploadedVariantImages: Map<string, ProductVariantImage>,
) {
  if (uploadedVariantImages.size !== data.variants.length) {
    throw new Error("Variant images are required for every variant.");
  }

  return data.variants.map((variant, index) => {
    const uploadedVariantImage = uploadedVariantImages.get(variant.clientKey);

    if (!uploadedVariantImage) {
      throw new Error("Variant image is required for every variant.");
    }

    return {
      name: buildVariationVariantName(variations, variant.selectedOptions),
      sku: variant.sku,
      costPrice: variant.costPrice,
      sellingPrice: variant.sellingPrice,
      stock: variant.stock,
      position: variant.position ?? index,
      image: uploadedVariantImage,
      selectedOptions: sortSelectedOptions(variations, variant.selectedOptions),
    };
  });
}

function mapUpdateVariationVariants(
  data: UpdateProductFormData,
  variations: ProductVariation[],
  uploadedVariantImages: ProductVariantImage[],
) {
  let uploadedVariantImageIndex = 0;

  return data.variants.map((variant, index) => {
    const image =
      variant.existImage ??
      (() => {
        const uploadedVariantImage = uploadedVariantImages[uploadedVariantImageIndex];

        if (!uploadedVariantImage) {
          throw new Error("Variant image is required for every variant.");
        }

        uploadedVariantImageIndex += 1;

        return uploadedVariantImage;
      })();

    return {
      id: variant.id,
      name: buildVariationVariantName(variations, variant.selectedOptions),
      sku: variant.sku,
      costPrice: variant.costPrice,
      sellingPrice: variant.sellingPrice,
      stock: variant.stock,
      position: variant.position ?? index,
      image,
      selectedOptions: sortSelectedOptions(variations, variant.selectedOptions),
    };
  });
}

export const toCreateProductCommand = (
  data: CreateProductFormData,
  uploadedProductImages: ProductImage[] = [],
  uploadedVariantImages: Map<string, ProductVariantImage> = new Map(),
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

  const variations = getCommandVariations(data);

  const variants = data.hasVariation
    ? mapCreateVariationVariants(data, variations, uploadedVariantImages)
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
            stock: data.baseStock,
            position: 0,
            image: fallbackVariantImage,
            selectedOptions: [],
          },
        ];
      })();

  return productBaseSchema.parse({
    name: data.name,
    description: data.description,
    supplierCountry: data.supplierCountry,
    franchiseId: data.franchiseId,
    warehouseId: data.warehouseId,
    variations,
    variants,
    productImages,
    isPublished: data.isPublished,
    hasVariation: data.hasVariation,
  });
};

export const toCreateProductMultipartFormData = (
  data: CreateProductFormData,
) => {
  const multipartFormData = new FormData();
  multipartFormData.append(
    "payload",
    JSON.stringify({
      ...data,
      variations: data.variations.map((variation) => ({
        ...variation,
        options: variation.options.map((option) => ({
          ...option,
        })),
      })),
      variants: data.variants.map((variant) => ({
        ...variant,
        image: variant.image
          ? {
              alt: variant.image.alt,
            }
          : undefined,
      })),
      productImages: data.productImages.map((image) => ({
        alt: image.alt,
        position: image.position,
      })),
    }),
  );

  for (const image of data.productImages) {
    multipartFormData.append("images", image.file);
  }

  for (const variant of data.variants) {
    if (variant.image?.file) {
      multipartFormData.append(
        `variantImages:${variant.clientKey}`,
        variant.image.file,
      );
    }
  }

  return multipartFormData;
};

export const toUpdateProductMultipartFormData = (
  data: UpdateProductFormData,
) => {
  const multipartFormData = new FormData();

  multipartFormData.append(
    "payload",
    JSON.stringify({
      ...data,
      variations: data.variations.map((variation) => ({
        ...variation,
        options: variation.options.map((option) => ({
          ...option,
        })),
      })),
      variants: data.variants.map((variant) => ({
        ...variant,
        existImage: variant.existImage
          ? {
              path: variant.existImage.path,
              alt: variant.existImage.alt,
            }
          : undefined,
        newImage: variant.newImage
          ? {
              alt: variant.newImage.alt,
            }
          : undefined,
      })),
      existProductImages: data.existProductImages.map((image) => ({
        path: image.path,
        alt: image.alt,
        position: image.position,
      })),
      newProductImages: data.newProductImages.map((image) => ({
        alt: image.alt,
        position: image.position,
      })),
    }),
  );

  for (const image of data.newProductImages) {
    multipartFormData.append("images", image.file);
  }

  for (const variant of data.variants) {
    if (variant.newImage) {
      multipartFormData.append("variantImages", variant.newImage.file);
    }
  }

  return multipartFormData;
};

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

  const variations = getCommandVariations(data);

  const variants = data.hasVariation
    ? mapUpdateVariationVariants(data, variations, uploadedVariantImages)
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
            id: data.baseVariantId,
            name: data.name,
            sku: data.baseSku,
            costPrice: data.baseCostPrice,
            sellingPrice: data.baseSellingPrice,
            stock: data.baseStock,
            position: 0,
            image: fallbackVariantImage,
            selectedOptions: [],
          },
        ];
      })();

  return updateProductCommandSchema.parse({
    name: data.name,
    description: data.description,
    supplierCountry: data.supplierCountry,
    franchiseId: data.franchiseId,
    warehouseId: data.warehouseId,
    variations,
    variants,
    productImages,
    isPublished: data.isPublished,
    hasVariation: data.hasVariation,
  });
};

function mapProductVariationsToFormInput(product: ProductWithRelations) {
  return [...product.variations]
    .sort((a, b) => a.position - b.position)
    .map((variation) => ({
      id: variation.id,
      name: variation.name,
      position: variation.position,
      options: [...variation.options]
        .sort((a, b) => a.position - b.position)
        .map((option) => ({
          id: option.id,
          value: option.value,
          position: option.position,
        })),
    }));
}

function mapVariantSelectedOptionsToFormInput(
  product: ProductWithRelations,
  variant: ProductWithRelations["variants"][number],
) {
  const positionsByVariationId = new Map(
    product.variations.map((variation) => [variation.id, variation.position]),
  );

  return [...variant.variationOptions]
    .sort((left, right) => {
      return (
        (positionsByVariationId.get(left.variationId) ?? 0) -
        (positionsByVariationId.get(right.variationId) ?? 0)
      );
    })
    .map((selection) => ({
      variationId: selection.variationId,
      optionId: selection.variationOptionId,
    }));
}

export const toUpdateProductCommandInput = (
  product: ProductWithRelations,
): UpdateProductFormInputData => {
  const variants = [...product.variants].sort(
    (a, b) => a.position - b.position,
  );
  const hasVariation = product.hasVariation;
  const baseVariant = variants[0];
  const inventoryState = getProductInventoryState(product);
  const variations = hasVariation ? mapProductVariationsToFormInput(product) : [];
  const hasExplicitVariationRelations = variations.length > 0;

  return {
    name: product.name,
    description: product.description ?? undefined,
    supplierCountry: product.supplierCountry,
    franchiseId: product.franchiseId ?? undefined,
    warehouseId: inventoryState.warehouseId,
    hasVariation,
    baseVariantId: hasVariation ? undefined : baseVariant.id,
    baseSku: hasVariation ? undefined : baseVariant.sku,
    baseCostPrice: hasVariation ? undefined : baseVariant.costPrice,
    baseSellingPrice: hasVariation ? undefined : baseVariant.sellingPrice,
    baseStock: hasVariation
      ? undefined
      : {
          quantity: inventoryState.variantQuantityById.get(baseVariant.id) ?? 0,
        },
    variations,
    variants: hasVariation
      ? variants.map((variant) => ({
          id: variant.id,
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
          stock: {
            quantity: inventoryState.variantQuantityById.get(variant.id) ?? 0,
          },
          position: variant.position,
          selectedOptions: hasExplicitVariationRelations
            ? mapVariantSelectedOptionsToFormInput(product, variant)
            : [],
          newImage: undefined,
          existImage: {
            path: variant.image!.path,
            alt: variant.image!.alt ?? undefined,
          },
        }))
      : [],
    newProductImages: [],
    existProductImages: product.productImages.map((image) => ({
      path: image.path,
      alt: image.alt ?? undefined,
      position: image.position,
    })),
  };
};

export type CreateProductCommandData = z.output<typeof productBaseSchema>;
export type UpdateProductCommandData = z.output<
  typeof updateProductCommandSchema
>;
