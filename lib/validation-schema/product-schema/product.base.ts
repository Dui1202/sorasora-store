import { SupplierCountry } from "@/app/generated/prisma/enums";
import { z } from "zod";

export const supplierCountrySchema = z.enum(SupplierCountry);

//Product Variant Image validation schema
export const productVariantImageBaseSchema = z.object({
  path: z.string().trim().min(1, "Image path is required"),
  alt: z.string().trim().optional(),
});

//Product Image validation schema
export const productImageBaseSchema = z.object({
  path: z.string().trim().min(1, "Image path is required"),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

//Product Variant validation schema
export const productVariantBaseSchema = z.object({
  name: z.string().trim().min(1, "Variant name is required"),
  sku: z.string().trim().min(1, "SKU is required"),
  costPrice: z.number().int().min(0, "Original price must be 0 or greater"),
  sellingPrice: z.number().int().min(0, "Sale price must be 0 or greater"),
  position: z.coerce.number().int().min(0).default(0),
  image: productVariantImageBaseSchema,
});

//Product base validation schema (shared properties)
export const productBaseSchema = z.object({
  name: z.string().trim().min(1, "Product name is required"),
  description: z.string().trim().optional(),
  supplierCountry: supplierCountrySchema,
  franchiseId: z.cuid("Invalid franchise Id").optional(),
  isPublished: z.boolean().default(false),
  variants: z.array(productVariantBaseSchema).min(1),
  productImages: z
    .array(productImageBaseSchema)
    .min(1, "Upload at least 1 product image!"),
});

export type ProductImages = z.infer<typeof productImageBaseSchema>[];
export type ProductVariantImages = z.infer<
  typeof productVariantImageBaseSchema
>[];
