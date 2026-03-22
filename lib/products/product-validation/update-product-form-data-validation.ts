import z from "zod";
import { updateProductFormSchema } from "../../validation-schema/product-schema";
import { multipartPayloadUpdateProductSchema } from "../../validation-schema/product-schema/product-api";

type UpdateProductFormValidationResult =
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

  const newImagesFromPayloads = payloadValidation.data.productImages.map(
    (image) => !("path" in image),
  );

  if (imageFiles.length !== newImagesFromPayloads.length) {
    return {
      success: false,
      error: "Image files count does not match product metadata.",
    };
  }

  const newVariantImagesFormPayloads = payloadValidation.data.variants.map(
    (variant) => !("path" in variant.image),
  );

  if (
    payloadValidation.data.hasVariation &&
    variantImageFiles.length !== newVariantImagesFormPayloads.length
  ) {
    return {
      success: false,
      error: "Variant image files count does not match variant metadata.",
    };
  }

  const formDataCandidate = {
    ...payloadValidation.data,
    variants: payloadValidation.data.variants.map((variant, index) => ({
      ...variant,
      image:
        payloadValidation.data.hasVariation && variantImageFiles[index]
          ? {
              file: variantImageFiles[index],
              alt: variant.image?.alt,
            }
          : undefined,
    })),
    productImages: payloadValidation.data.productImages.map((image, index) => ({
      file: imageFiles[index],
      alt: image.alt,
      position: image.position,
    })),
  };

  const formValidation = updateProductFormSchema.safeParse(formDataCandidate);

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
