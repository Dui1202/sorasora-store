import { z } from "zod";

const supplierCountrySchema = z.enum(["CHINA", "JAPAN"]);

const createProductVariantImageSchema = z.object({
  url: z.string().trim().pipe(z.url("Image URL must be a valid URL")),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

const createProductVariantSchema = z.object({
  name: z.string().trim().min(1, "Variant name is required"),
  sku: z.string().trim().min(1, "SKU is required"),
  originalPrice: z.coerce
    .number()
    .int()
    .min(0, "Original price must be 0 or greater"),
  salePrice: z.coerce.number().int().min(0, "Sale price must be 0 or greater"),
  images: z
    .array(createProductVariantImageSchema)
    .min(1, "Required at least 1 image"),
});

export const createProductFormSchema = z.object({
  name: z.string().trim().min(1, "Product name is required"),
  description: z.string().trim().optional(),
  supplierCountry: supplierCountrySchema,
  franchiseId: z.cuid("Invalid franchise id").optional(),
  variants: z.array(createProductVariantSchema).default([]),
});

export type CreateProductFormInputData = z.input<
  typeof createProductFormSchema
>;
export type CreateProductFormOutputData = z.output<
  typeof createProductFormSchema
>;
