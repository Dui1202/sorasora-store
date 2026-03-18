import z from "zod";
import { createProductFormBaseSchema } from "./create-product-form";

// Response validation schema for uploaded product image
export const uploadedProductImageSchema = z.object({
  path: z.string().min(1),
});

export const multipartPayloadVariantSchema =
  createProductFormBaseSchema.shape.variants
    .unwrap()
    .element.omit({ image: true })
    .extend({
      image: z
        .object({
          alt: z.string().trim().optional(),
        })
        .optional(),
    });

export const multipartPayloadSchema = createProductFormBaseSchema
  .omit({ productImages: true, variants: true })
  .extend({
    variants: z.array(multipartPayloadVariantSchema).default([]),
    productImages: z
      .array(
        z.object({
          alt: z.string().trim().optional(),
          position: z.coerce.number().int().min(0).default(0),
        }),
      )
      .min(1, "Upload at least 1 product image!"),
  });

export type UploadProductImageResponse = z.infer<
  typeof uploadedProductImageSchema
>;
