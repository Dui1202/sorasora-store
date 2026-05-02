import z from "zod";
import {
  productBaseSchema,
  productVariantBaseSchema,
  stockBaseSchema,
  supplierCountrySchema,
} from "./product-base";
import {
  productVariationBaseSchema,
  validateVariationConfiguration,
} from "./variation-contract";

// For image preview before upload, we use a local schema that includes the File object.
export const localProductImageSchema = z.object({
  file: z.instanceof(File),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

export const localProductVariantImageSchema = z.object({
  file: z.instanceof(File),
  alt: z.string().trim().optional(),
});

// Product variant form validation schema
const createProductVariantFormSchema = productVariantBaseSchema
  .omit({
    image: true,
  })
  .extend({
    clientKey: z.string().trim().min(1, "Variant client key is required"),
    image: localProductVariantImageSchema.optional(),
  });

// Main create product form validation schema
export const createProductFormBaseSchema = productBaseSchema
  .omit({
    supplierCountry: true,
    franchiseId: true,
    warehouseId: true,
    isPublished: true,
    hasVariation: true,
    productImages: true,
    variants: true,
  })
  .extend({
    supplierCountry: supplierCountrySchema.optional(),
    franchiseId: z.cuid("Invalid franchise Id").optional(),
    warehouseId: z.cuid("Invalid warehouse Id").optional(),
    isPublished: z.boolean().default(false),
    hasVariation: z.boolean().default(false),
    baseSku: z.string().trim().optional(),
    baseCostPrice: z
      .number()
      .int()
      .min(0, "Original price must be 0 or greater")
      .optional(),
    baseSellingPrice: z
      .number()
      .int()
      .min(0, "Sale price must be 0 or greater")
      .optional(),
    baseStock: stockBaseSchema.optional(),
    variations: z.array(productVariationBaseSchema).default([]),
    variants: z.array(createProductVariantFormSchema).default([]),
    productImages: z.array(localProductImageSchema).default([]),
  });

// Add extra validation for the createProductFormBaseSchema
export const createProductFormSchema = createProductFormBaseSchema.superRefine(
  (data, ctx) => {
    if (!data.supplierCountry) {
      ctx.addIssue({
        code: "custom",
        message: "Supplier country is required",
        path: ["supplierCountry"],
      });
    }

    if (!data.warehouseId) {
      ctx.addIssue({
        code: "custom",
        message: "Warehouse is required",
        path: ["warehouseId"],
      });
    }

    if (data.hasVariation) {
      validateVariationConfiguration(data, ctx);

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

    if (!data.baseStock) {
      ctx.addIssue({
        code: "custom",
        message: "Stock quantity must be 0 or greater",
        path: ["baseStock", "quantity"],
      });
    }
  },
);

export type CreateProductFormData = z.output<
  typeof createProductFormBaseSchema
>;
export type CreateProductFormInputData = z.input<
  typeof createProductFormBaseSchema
>;
export type CreateProductFormValidationData = z.infer<
  typeof createProductFormSchema
>;
export type LocalProductImage = z.infer<typeof localProductImageSchema>;
