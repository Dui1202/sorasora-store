import z from "zod";
import {
  localProductImageSchema,
  localProductVariantImageSchema,
} from "./create-product-form";
import {
  productBaseSchema,
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
const updateProductFormBaseSchema = productBaseSchema
  .omit({
    productImages: true,
    variants: true,
  })
  .extend({
    productImages: z.array(updateProductImageSchema).min(1),
    variants: z.array(updateProductVariantSchema).min(1),
  });

// For zod resolver schema
export const updateProductFormSchema = updateProductFormBaseSchema;

export type UpdateProductFormInputData = z.input<
  typeof updateProductFormBaseSchema
>;

export type UpdateProductFormData = z.output<
  typeof updateProductFormBaseSchema
>;
