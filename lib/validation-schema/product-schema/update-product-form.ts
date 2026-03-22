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

// Update product image validation schema
const updateProductImageSchema = z.union([
  localProductImageSchema,
  productImageBaseSchema,
]);

// Update product variant image validation schema
const updateProductVariantImageSchema = z.union([
  localProductVariantImageSchema,
  productVariantImageBaseSchema,
]);

// Update product variant form validation schema
const updateProductVariantSchema = productVariantBaseSchema
  .omit({
    image: true,
  })
  .extend({
    /// For non-image edits
    image: updateProductVariantImageSchema.optional(),
  });

// Main update product form validation schema
export const updateProductFormBaseSchema = createProductFormBaseSchema
  .omit({
    productImages: true,
    variants: true,
  })
  .extend({
    productImages: z.array(updateProductImageSchema).min(1),
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

      data.variants.forEach((variant, index) => {
        if (!variant.image) {
          ctx.addIssue({
            code: "custom",
            message: "Variant image is required.",
            path: ["variants", index, "image"],
          });
        }
      });

      return;
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
  },
);

export type UpdateProductFormInputData = z.input<
  typeof updateProductFormBaseSchema
>;

export type UpdateProductFormData = z.output<
  typeof updateProductFormBaseSchema
>;

export type UpdateProductImage = z.infer<typeof updateProductImageSchema>;
export type UpdateProductVariantImage = z.infer<
  typeof updateProductVariantImageSchema
>;
