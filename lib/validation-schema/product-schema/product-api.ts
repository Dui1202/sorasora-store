import z from "zod";
import { createProductFormBaseSchema } from "./create-product-form";
import { updateProductFormBaseSchema } from "./update-product-form";
import {
  productImageBaseSchema,
  productVariantImageBaseSchema,
} from "./product-base";

// Response validation schema for uploaded product image
export const uploadedProductImageSchema = z.object({
  path: z.string().min(1),
});

//**Create Product Payload**
export const multipartPayloadCreateProductVariantSchema =
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

export const multipartPayloadCreateProductSchema = createProductFormBaseSchema
  .omit({ productImages: true, variants: true })
  .extend({
    variants: z.array(multipartPayloadCreateProductVariantSchema).default([]),
    productImages: z
      .array(
        z.object({
          alt: z.string().trim().optional(),
          position: z.coerce.number().int().min(0).default(0),
        }),
      )
      .min(1, "Upload at least 1 product image!"),
  });

//**Update Product Payload**

///Payload fields for Variant Image
const existingVariantImageMetadata = productVariantImageBaseSchema;
const newVariantImageMetadata = productVariantImageBaseSchema.omit({
  path: true,
});

///Payload fields for Product Image
const existingImageMetadata = productImageBaseSchema;
const newImageMetadata = productImageBaseSchema.omit({ path: true });

///Payload fields for Product Variant
export const multipartPayloadUpdateProductVariantSchema =
  updateProductFormBaseSchema.shape.variants
    .unwrap()
    .element.omit({ image: true })
    .extend({
      image: z.union([existingVariantImageMetadata, newVariantImageMetadata]),
    });

///Payload fields for Product
export const multipartPayloadUpdateProductSchema = updateProductFormBaseSchema
  .omit({
    productImages: true,
    variants: true,
  })
  .extend({
    variants: z.array(multipartPayloadUpdateProductVariantSchema).default([]),
    productImages: z
      .array(z.union([existingImageMetadata, newImageMetadata]))
      .min(1, "Upload at least 1 product image!"),
  });

export type UploadProductImageResponse = z.infer<
  typeof uploadedProductImageSchema
>;
