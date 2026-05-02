import { z } from "zod";

const normalizeVariationValue = (value: string) => value.trim().toLowerCase();

export const variationSelectionSchema = z.object({
  variationId: z.string().trim().min(1, "Variation id is required"),
  optionId: z.string().trim().min(1, "Variation option id is required"),
});

export const productVariationOptionBaseSchema = z.object({
  id: z.string().trim().min(1, "Variation option id is required"),
  value: z.string().trim().min(1, "Variation option value is required"),
  position: z.coerce.number().int().min(0).default(0),
});

export const productVariationBaseSchema = z.object({
  id: z.string().trim().min(1, "Variation id is required"),
  name: z.string().trim().min(1, "Variation name is required"),
  position: z.coerce.number().int().min(0).default(0),
  options: z
    .array(productVariationOptionBaseSchema)
    .min(1, "Add at least 1 option"),
});

export type VariationSelection = z.infer<typeof variationSelectionSchema>;
export type ProductVariationOption = z.infer<
  typeof productVariationOptionBaseSchema
>;
export type ProductVariation = z.infer<typeof productVariationBaseSchema>;

type VariationValidationVariant = {
  selectedOptions?: VariationSelection[];
};

export function buildVariationSelectionSignature(
  variations: ProductVariation[],
  selectedOptions: VariationSelection[],
) {
  const optionsByVariationId = new Map(
    variations.map((variation) => [
      variation.id,
      {
        name: variation.name,
        optionsById: new Map(
          variation.options.map((option) => [option.id, option.value]),
        ),
      },
    ]),
  );

  return selectedOptions
    .map((selection) => {
      const variation = optionsByVariationId.get(selection.variationId);
      const optionValue = variation?.optionsById.get(selection.optionId);

      if (!variation || !optionValue) {
        return null;
      }

      return `${normalizeVariationValue(variation.name)}:${normalizeVariationValue(optionValue)}`;
    })
    .join("|");
}

export function buildVariationDisplayName(
  variations: ProductVariation[],
  selectedOptions: VariationSelection[],
) {
  const variationsById = new Map(variations.map((variation) => [variation.id, variation]));

  return selectedOptions
    .map((selection, index) => {
      const variation = variationsById.get(selection.variationId);
      const option = variation?.options.find(
        (variationOption) => variationOption.id === selection.optionId,
      );

      if (!variation || !option) {
        throw new Error(
          `Cannot build variation display name for selection at index ${index}.`,
        );
      }

      return `${variation.name}: ${option.value}`;
    })
    .join(" / ");
}

export function validateVariationConfiguration(
  data: {
    hasVariation: boolean;
    variations: ProductVariation[];
    variants: VariationValidationVariant[];
  },
  ctx: z.RefinementCtx,
) {
  if (!data.hasVariation) {
    if (data.variations.length > 0) {
      ctx.addIssue({
        code: "custom",
        message: "Variations must be empty when variations are disabled.",
        path: ["variations"],
      });
    }

    return;
  }

  if (data.variations.length < 1) {
    ctx.addIssue({
      code: "custom",
      message: "Add at least 1 variation when variations are enabled.",
      path: ["variations"],
    });
    return;
  }

  const seenVariationNames = new Set<string>();
  const variationIds = new Set<string>();
  const optionsByVariationId = new Map<string, Set<string>>();

  data.variations.forEach((variation, variationIndex) => {
    const normalizedVariationName = normalizeVariationValue(variation.name);

    if (seenVariationNames.has(normalizedVariationName)) {
      ctx.addIssue({
        code: "custom",
        message: "Variation names must be unique.",
        path: ["variations", variationIndex, "name"],
      });
    }

    seenVariationNames.add(normalizedVariationName);
    variationIds.add(variation.id);

    const optionIds = new Set<string>();
    const normalizedOptionValues = new Set<string>();

    variation.options.forEach((option, optionIndex) => {
      optionIds.add(option.id);

      const normalizedOptionValue = normalizeVariationValue(option.value);

      if (normalizedOptionValues.has(normalizedOptionValue)) {
        ctx.addIssue({
          code: "custom",
          message: "Variation option values must be unique.",
          path: ["variations", variationIndex, "options", optionIndex, "value"],
        });
      }

      normalizedOptionValues.add(normalizedOptionValue);
    });

    optionsByVariationId.set(variation.id, optionIds);
  });

  if (data.variants.length < 1) {
    ctx.addIssue({
      code: "custom",
      message: "Add at least 1 variant when variations are enabled.",
      path: ["variants"],
    });
    return;
  }

  const signatures = new Set<string>();

  data.variants.forEach((variant, variantIndex) => {
    const selectedOptions = variant.selectedOptions ?? [];

    if (selectedOptions.length !== data.variations.length) {
      ctx.addIssue({
        code: "custom",
        message: "Each variant must select exactly one option for each variation.",
        path: ["variants", variantIndex, "selectedOptions"],
      });
      return;
    }

    const seenVariationSelections = new Set<string>();

    for (const selection of selectedOptions) {
      if (!variationIds.has(selection.variationId)) {
        ctx.addIssue({
          code: "custom",
          message: "Selected variation does not exist.",
          path: ["variants", variantIndex, "selectedOptions"],
        });
        return;
      }

      if (seenVariationSelections.has(selection.variationId)) {
        ctx.addIssue({
          code: "custom",
          message: "Each variant can only select one option per variation.",
          path: ["variants", variantIndex, "selectedOptions"],
        });
        return;
      }

      seenVariationSelections.add(selection.variationId);

      const optionIds = optionsByVariationId.get(selection.variationId);

      if (!optionIds?.has(selection.optionId)) {
        ctx.addIssue({
          code: "custom",
          message: "Selected variation option does not exist.",
          path: ["variants", variantIndex, "selectedOptions"],
        });
        return;
      }
    }

    const signature = buildVariationSelectionSignature(
      data.variations,
      selectedOptions,
    );

    if (signatures.has(signature)) {
      ctx.addIssue({
        code: "custom",
        message: "Each variation combination must be unique.",
        path: ["variants", variantIndex, "selectedOptions"],
      });
    }

    signatures.add(signature);
  });
}
