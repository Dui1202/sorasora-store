import { SupplierCountry } from "@/app/generated/prisma/enums";
import { z } from "zod";

const supplierCountrySchema = z.enum(SupplierCountry);

// For image preview before upload, we use a local schema that includes the File object.
const localProductImageSchema = z.object({
  file: z.instanceof(File),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

//Product Variant Image validation schema
const createProductVariantImageSchema = z.object({
  path: z.string().trim().min(1, "Image path is required"),
  alt: z.string().trim().optional(),
});

//Product Image validation schema
const createProductImageSchema = z.object({
  path: z.string().trim().min(1, "Image path is required"),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

const localProductVariantImageSchema = z.object({
  file: z.instanceof(File),
  alt: z.string().trim().optional(),
});

//Product Variant validation schema
const createProductVariantSchema = z.object({
  name: z.string().trim().min(1, "Variant name is required"),
  sku: z.string().trim().min(1, "SKU is required"),
  costPrice: z.number().int().min(0, "Original price must be 0 or greater"),
  sellingPrice: z.number().int().min(0, "Sale price must be 0 or greater"),
  position: z.coerce.number().int().min(0).default(0),
  image: createProductVariantImageSchema,
});

const createProductVariantFormSchema = createProductVariantSchema
  .omit({
    image: true,
  })
  .extend({
    image: localProductVariantImageSchema.optional(),
});

//Product Form validation schema
// 1) Form boundary (UI raw shape)
export const createProductFormSchema = z.object({
  name: z.string().trim().min(1, "Product name is required"),
  description: z.string().trim().optional(),
  supplierCountry: supplierCountrySchema,
  franchiseId: z.cuid("Invalid franchise id").optional(),
  hasVariation: z.boolean(),
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
  variants: z.array(createProductVariantFormSchema).default([]),
  productImages: z.array(localProductImageSchema).default([]),
}).superRefine((data, ctx) => {
  if (data.hasVariation) {
    if (data.variants.length < 1) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Add at least 1 variant when variations are enabled.",
        path: ["variants"],
      });
    }

    data.variants.forEach((variant, index) => {
      if (!variant.image) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Variant image is required.",
          path: ["variants", index, "image"],
        });
      }
    });

    return;
  }

  if (!data.baseSku) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "SKU is required",
      path: ["baseSku"],
    });
  }

  if (data.baseCostPrice === undefined) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Original price must be 0 or greater",
      path: ["baseCostPrice"],
    });
  }

  if (data.baseSellingPrice === undefined) {
    ctx.addIssue({
      code: z.ZodIssueCode.custom,
      message: "Sale price must be 0 or greater",
      path: ["baseSellingPrice"],
    });
  }
});

// 2) API boundary (backend contract shape)
export const createProductCommandSchema = z.object({
  name: z.string().trim().min(1),
  description: z.string().trim().optional(),
  supplierCountry: supplierCountrySchema,
  franchiseId: z.cuid().optional(),
  variants: z.array(createProductVariantSchema).min(1),
  productImages: z
    .array(createProductImageSchema)
    .min(1, "Upload at least 1 product image!"),
});

// 3) Explicit mapper
export const toCreateProductCommand = (
  data: CreateProductFormData,
  uploadedProductImages: ProductImages = [],
  uploadedVariantImages: ProductVariantImages = [],
): CreateProductCommandData => {
  const productImages = createProductCommandSchema.shape.productImages.parse(
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
          throw new Error("Base variant fields are required when variation is disabled.");
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

  return createProductCommandSchema.parse({
    name: data.name,
    description: data.description,
    supplierCountry: data.supplierCountry,
    franchiseId: data.franchiseId,
    variants,
    productImages,
  });
};

// Response validation schema for uploaded product image
export const uploadedProductImageSchema = z.object({
  path: z.string().min(1),
});

export type UploadProductImageResponse = z.infer<
  typeof uploadedProductImageSchema
>;

export type CreateProductFormData = z.output<typeof createProductFormSchema>;
export type CreateProductFormInputData = z.input<
  typeof createProductFormSchema
>;
export type CreateProductCommandData = z.output<
  typeof createProductCommandSchema
>;

export type LocalProductImages = z.infer<typeof localProductImageSchema>[];
export type ProductImages = z.infer<typeof createProductImageSchema>[];
export type ProductVariantImages = z.infer<typeof createProductVariantImageSchema>[];
