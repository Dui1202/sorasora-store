import { SupplierCountry } from "@/app/generated/prisma/enums";
import { z } from "zod";

const supplierCountrySchema = z.enum(SupplierCountry);

const createProductVariantImageSchema = z.object({
  url: z.string().trim().pipe(z.url("Image URL must be a valid URL")),
  alt: z.string().trim().optional(),
});

const createProductImageSchema = z.object({
  url: z.string().trim().pipe(z.url("Image URL must be a valid URL")),
  alt: z.string().trim().optional(),
  position: z.coerce.number().int().min(0).default(0),
});

const createProductVariantSchema = z.object({
  name: z.string().trim().min(1, "Variant name is required"),
  sku: z.string().trim().min(1, "SKU is required"),
  originalPrice: z.number().int().min(0, "Original price must be 0 or greater"),
  salePrice: z.number().int().min(0, "Sale price must be 0 or greater"),
  position: z.coerce.number().int().min(0).default(0),
  image: createProductVariantImageSchema,
});

// 1) Form boundary (UI raw shape)
export const createProductFormSchema = z.object({
  name: z.string().trim().min(1, "Product name is required"),
  description: z.string().trim().optional(),
  supplierCountry: supplierCountrySchema,
  franchiseId: z.cuid("Invalid franchise id").optional(),
  hasVariation: z.boolean(),
  baseSku: z.string().trim().min(1, "SKU is required"),
  baseOriginalPrice: z
    .number()
    .int()
    .min(0, "Original price must be 0 or greater"),
  baseSalePrice: z.number().int().min(0, "Sale price must be 0 or greater"),
  variants: z.array(createProductVariantSchema).default([]),
  productImages: z.array(createProductImageSchema).default([]),
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
): CreateProductCommandData => {
  const variants = data.hasVariation
    ? data.variants
    : [
        {
          name: data.name,
          sku: data.baseSku,
          originalPrice: data.baseOriginalPrice,
          salePrice: data.baseSalePrice,
          position: 0,
          image: data.productImages[0],
        },
      ];

  return createProductCommandSchema.parse({
    name: data.name,
    description: data.description,
    supplierCountry: data.supplierCountry,
    franchiseId: data.franchiseId,
    variants,
    productImages: data.productImages,
  });
};

export type CreateProductFormData = z.output<typeof createProductFormSchema>;
export type CreateProductFormInputData = z.input<
  typeof createProductFormSchema
>;
export type CreateProductCommandData = z.output<
  typeof createProductCommandSchema
>;

export type ProductImages = z.infer<typeof createProductImageSchema>[];
