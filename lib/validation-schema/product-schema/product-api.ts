import z from "zod";
import {
  createProductFormBaseSchema,
  localProductImageSchema,
} from "./create-product-form";
import {
  updateProductFormBaseSchema,
  updateProductVariantBaseSchema,
} from "./update-product-form";
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

///Payload fields for Product Variant

export const multipartPayloadUpdateProductVariantSchema =
  updateProductVariantBaseSchema.omit({ newImage: true }).extend({
    newImage: z
      .object({
        alt: z.string().trim().optional(),
      })
      .optional(),
  });

///Payload fields for Product

const newProductImagePayloadSchema = localProductImageSchema.omit({
  file: true,
});

export const multipartPayloadUpdateProductSchema = updateProductFormBaseSchema
  .omit({
    newProductImages: true,
    variants: true,
  })
  .extend({
    variants: z.array(multipartPayloadUpdateProductVariantSchema).default([]),
    newProducImages: z.array(newProductImagePayloadSchema).optional(),
  });

export type UploadProductImageResponse = z.infer<
  typeof uploadedProductImageSchema
>;
