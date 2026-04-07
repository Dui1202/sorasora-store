import { AppError } from "@/core/errors/errors";
import z from "zod";
import { updateProductFormSchema } from "../schema";
import { multipartPayloadUpdateProductSchema } from "../schema/product-api";
import { createMultipartValidationError } from "./multipart-validation-error";

export type UpdateProductFormValidationResult =
  | {
      success: true;
      data: z.infer<typeof updateProductFormSchema>;
    }
  | {
      success: false;
      error: AppError;
    };

export const validateUpdateProductFormData = (
  formData: FormData,
): UpdateProductFormValidationResult => {
  const payloadField = formData.get("payload");

  if (typeof payloadField !== "string") {
    return {
      success: false,
      error: createMultipartValidationError("Missing payload field"),
    };
  }

  let parsedPayload: unknown;

  // Validate the payload structure before processing files
  try {
    parsedPayload = JSON.parse(payloadField);
  } catch {
    return {
      success: false,
      error: createMultipartValidationError("Invalid payload format"),
    };
  }

  // Validate the payload content against the schema (excluding files)
  const payloadValidation =
    multipartPayloadUpdateProductSchema.safeParse(parsedPayload);

  if (!payloadValidation.success) {
    return {
      success: false,
      error: createMultipartValidationError("Invalid payload format", {
        source: "payload",
        issues: z.flattenError(payloadValidation.error),
      }),
    };
  }

  // Get image files and validate their count against the payload metadata
  const imageFiles = formData
    .getAll("images")
    .filter((value): value is File => value instanceof File);
  const variantImageFiles = formData
    .getAll("variantImages")
    .filter((value): value is File => value instanceof File);

  if (imageFiles.length !== payloadValidation.data.newProductImages?.length) {
    return {
      success: false,
      error: createMultipartValidationError(
        "Image files count does not match product metadata.",
        {
          expected: payloadValidation.data.newProductImages?.length ?? 0,
          received: imageFiles.length,
        },
      ),
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
      error: createMultipartValidationError(
        "Variant image files count does not match variant metadata.",
        {
          expected: payloadValidation.data.variants.filter(
            (variant) => variant.newImage,
          ).length,
          received: variantImageFiles.length,
        },
      ),
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
        alt: payloadValidation.data.newProductImages?.[index]?.alt,
        position:
          payloadValidation.data.newProductImages?.[index]?.position ?? 0,
      })),
    };
  };

  const formValidation = updateProductFormSchema.safeParse(formDataCandidate());

  if (!formValidation.success) {
    return {
      success: false,
      error: createMultipartValidationError("Invalid form data", {
        source: "formDataCandidate",
        issues: z.flattenError(formValidation.error),
      }),
    };
  }

  return {
    success: true,
    data: formValidation.data,
  };
};
