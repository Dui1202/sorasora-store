import z from "zod";
import {
  createProductFormBaseSchema,
  localProductImageSchema,
  localProductVariantImageSchema,
} from "./create-product-form";
import {
  productImageBaseSchema,
  productVariantBaseSchema,
  productVariantImageBaseSchema,
} from "./product-base";

export const updateProductVariantBaseSchema = productVariantBaseSchema
  .omit({
    image: true,
  })
  .extend({
    existImage: productVariantImageBaseSchema.optional(),
    newImage: localProductVariantImageSchema.optional(),
  });

// Update product variant validation schema
const updateProductVariantSchema = updateProductVariantBaseSchema.superRefine(
  (data, ctx) => {
    if (
      (!data.existImage && !data.newImage) ||
      (data.existImage && data.newImage)
    ) {
      ctx.addIssue({
        code: "custom",
        message: "Variant must have exactly one image",
        path: ["existImage"],
      });

      ctx.addIssue({
        code: "custom",
        message: "Variant must have exactly one image",
        path: ["newImage"],
      });
    }
  },
);

// Main update product form validation schema
export const updateProductFormBaseSchema = createProductFormBaseSchema
  .omit({
    productImages: true,
    variants: true,
  })
  .extend({
    newProductImages: z.array(localProductImageSchema).default([]),
    existProductImages: z.array(productImageBaseSchema).default([]),
    variants: z.array(updateProductVariantSchema).default([]),
  });

// For zod resolver schema
export const updateProductFormSchema = updateProductFormBaseSchema.superRefine(
  (data, ctx) => {
    if (data.hasVariation) {
      if (data.variants.length < 1) {
        ctx.addIssue({
          code: "custom",
          message: "Add at least 1 variant when variations are enabled.",
          path: ["variants"],
        });
      }

      return;
    }

    if (data.variants.length > 0) {
      ctx.addIssue({
        code: "custom",
        message: "Variants must be empty when variations are disabled.",
        path: ["variants"],
      });
    }

    if (!data.baseSku) {
      ctx.addIssue({
        code: "custom",
        message: "SKU is required",
        path: ["baseSku"],
      });
    }

    if (data.baseCostPrice === undefined) {
      ctx.addIssue({
        code: "custom",
        message: "Original price must be 0 or greater",
        path: ["baseCostPrice"],
      });
    }

    if (data.baseSellingPrice === undefined) {
      ctx.addIssue({
        code: "custom",
        message: "Sale price must be 0 or greater",
        path: ["baseSellingPrice"],
      });
    }

    if (!data.newProductImages.length && !data.existProductImages.length) {
      ctx.addIssue({
        code: "custom",
        message: "Image is required",
        path: ["newProductImages"],
      });

      ctx.addIssue({
        code: "custom",
        message: "Image is required",
        path: ["existProductImages"],
      });
    }
  },
);

export type UpdateProductFormInputData = z.input<
  typeof updateProductFormBaseSchema
>;

export type UpdateProductFormData = z.output<
  typeof updateProductFormBaseSchema
>;
export type UpdateProductFormValidationData = z.infer<
  typeof updateProductFormSchema
>;
