import z from "zod";
import {
  LocalProductImage,
  updateProductFormSchema,
} from "../../validation-schema/product-schema";
import { multipartPayloadUpdateProductSchema } from "../../validation-schema/product-schema/product-api";
import { Product } from "@/app/generated/prisma/client";
import { ProductImage } from "@/lib/validation-schema/product-schema/product-base";

export type UpdateProductFormValidationResult =
  | {
      success: true;
      data: z.infer<typeof updateProductFormSchema>;
    }
  | {
      success: false;
      error: string;
    };

export const validateUpdateProductFormData = (
  formData: FormData,
): UpdateProductFormValidationResult => {
  const payloadField = formData.get("payload");

  if (typeof payloadField !== "string") {
    return {
      success: false,
      error: "Missing payload field",
    };
  }

  let parsedPayload: unknown;

  // Validate the payload structure before processing files
  try {
    parsedPayload = JSON.parse(payloadField);
  } catch {
    return {
      success: false,
      error: "Invalid payload format",
    };
  }

  // Validate the payload content against the schema (excluding files)
  const payloadValidation =
    multipartPayloadUpdateProductSchema.safeParse(parsedPayload);

  if (!payloadValidation.success) {
    return {
      success: false,
      error: "Invalid payload format",
    };
  }

  // Get image files and validate their count against the payload metadata
  const imageFiles = formData
    .getAll("images")
    .filter((value): value is File => value instanceof File);
  const variantImageFiles = formData
    .getAll("variantImages")
    .filter((value): value is File => value instanceof File);

  if (imageFiles.length !== payloadValidation.data.newProducImages?.length) {
    return {
      success: false,
      error: "Image files count does not match product metadata.",
    };
  }

  if (
    payloadValidation.data.hasVariation &&
    variantImageFiles.length !==
      payloadValidation.data.variants.filter((variant) => variant.newImage)
        .length
  ) {
    return {
      success: false,
      error: "Variant image files count does not match variant metadata.",
    };
  }

  const formDataCandidate = () => {
    let variantImageFileIndex = 0;

    return {
      ...payloadValidation.data,
      variants: payloadValidation.data.variants.map((variant) => {
        if (!variant.newImage) {
          return {
            ...variant,
            newImage: undefined,
          };
        }

        const file = variantImageFiles[variantImageFileIndex];

        if (!file) {
          throw new Error("Missing uploaded file for variant image.");
        }

        variantImageFileIndex += 1;

        return {
          ...variant,
          newImage: {
            file,
            alt: variant.newImage.alt,
          },
        };
      }),
      newProductImages: imageFiles.map((file, index) => ({
        file,
        alt: payloadValidation.data.newProducImages?.[index]?.alt,
        position:
          payloadValidation.data.newProducImages?.[index]?.position ?? 0,
      })),
    };
  };

  const formValidation = updateProductFormSchema.safeParse(formDataCandidate());

  if (!formValidation.success) {
    return {
      success: false,
      error: formValidation.error.message,
    };
  }

  return {
    success: true,
    data: formValidation.data,
  };
};
