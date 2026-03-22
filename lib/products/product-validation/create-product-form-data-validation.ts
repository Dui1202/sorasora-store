import {
  createProductFormSchema,
  multipartPayloadSchema,
} from "@/lib/validation-schema/product-schema";
import z from "zod";

type CreateProductFormValidationResult =
  | {
      success: true;
      data: z.infer<typeof createProductFormSchema>;
    }
  | {
      success: false;
      error: string;
    };

export const validateCreateProductFormData = (
  formData: FormData,
): CreateProductFormValidationResult => {
  // Handle payload parsing and validation
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
  const payloadValidation = multipartPayloadSchema.safeParse(parsedPayload);

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

  if (imageFiles.length !== payloadValidation.data.productImages.length) {
    return {
      success: false,
      error: "Image files count does not match product metadata.",
    };
  }

  if (
    payloadValidation.data.hasVariation &&
    variantImageFiles.length !== payloadValidation.data.variants.length
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

  // Final validation before processing the creation command
  const formValidation = createProductFormSchema.safeParse(formDataCandidate);

  if (!formValidation.success) {
    return {
      success: false,
      error: "Invalid form data",
    };
  }

  return {
    success: true,
    data: formValidation.data,
  };
};
