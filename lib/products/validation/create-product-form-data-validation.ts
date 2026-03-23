import {
  createProductFormSchema,
  CreateProductFormValidationData,
  multipartPayloadSchema,
} from "@/lib/validation-schema/product-schema";

type CreateProductFormValidationResult =
  | {
      success: true;
      data: CreateProductFormValidationData;
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
  const variantImageFilesByClientKey = new Map<string, File>();

  for (const [entryKey, entryValue] of formData.entries()) {
    if (!entryKey.startsWith("variantImages:")) {
      continue;
    }

    if (!(entryValue instanceof File)) {
      return {
        success: false,
        error: "Variant image payload must be a file.",
      };
    }

    const clientKey = entryKey.slice("variantImages:".length).trim();

    if (!clientKey) {
      return {
        success: false,
        error: "Variant image client key is missing.",
      };
    }

    if (variantImageFilesByClientKey.has(clientKey)) {
      return {
        success: false,
        error: "Duplicate variant image client key detected.",
      };
    }

    variantImageFilesByClientKey.set(clientKey, entryValue);
  }

  if (imageFiles.length !== payloadValidation.data.productImages.length) {
    return {
      success: false,
      error: "Image files count does not match product metadata.",
    };
  }

  if (
    payloadValidation.data.hasVariation &&
    variantImageFilesByClientKey.size !== payloadValidation.data.variants.length
  ) {
    return {
      success: false,
      error: "Variant image files count does not match variant metadata.",
    };
  }

  const formDataCandidate = {
    ...payloadValidation.data,
    variants: payloadValidation.data.variants.map((variant) => ({
      ...variant,
      image:
        payloadValidation.data.hasVariation &&
        variantImageFilesByClientKey.get(variant.clientKey)
          ? {
              file: variantImageFilesByClientKey.get(variant.clientKey)!,
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
