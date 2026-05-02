import { OptionInputObject } from "../option-input-group";
import {
  PriceFormVariationInput,
  CreatePriceFormVariantInput,
  PriceFormVariantImageInput,
  PriceFormVariantInput,
  UpdatePriceFormVariantInput,
} from "../types";
import { Variation } from "../variation-field-group/type";
import {
  createVariantSignature,
  normalizeSignatureValue,
  VariantDraftRow,
  VariantOptionSelection,
  VariantSignaturePart,
} from "./use-variant-combinations";
import { VariantDraftSeed } from "./use-variant-draft-rows";

export type NumericDraftField = "costPrice" | "sellingPrice" | "stock";

type ParsedVariantNamePart = {
  variationName: string;
  optionValue: string;
  variationIndex: number;
};

type ParsedVariantEntry = {
  parts: ParsedVariantNamePart[];
  signature: string;
  variant: PriceFormVariantInput;
};

export type ParsedVariantEditorData = {
  variations: Variation[];
  seeds: VariantDraftSeed[];
};

export type ParsedVariantEditorResult =
  | {
      success: true;
      data: ParsedVariantEditorData;
    }
  | {
      success: false;
      error: string;
    };

export function getVariationLabel(variation: Variation, index: number) {
  const variationName = variation.name.trim();
  return variationName || `Variation ${index + 1}`;
}

export function parseNonNegativeInt(rawValue: string) {
  const parsedValue = Number.parseInt(rawValue, 10);

  if (Number.isNaN(parsedValue) || parsedValue < 0) {
    return 0;
  }

  return parsedValue;
}

function parseVariantNameStrict(name: string): ParsedVariantNamePart[] | null {
  const parts = name.trim().split(" / ");

  if (parts.length < 1 || parts.length > 2) {
    return null;
  }

  const parsedParts: ParsedVariantNamePart[] = [];

  for (let index = 0; index < parts.length; index += 1) {
    const part = parts[index];
    const separatorIndex = part.indexOf(": ");

    if (separatorIndex <= 0) {
      return null;
    }

    const variationName = part.slice(0, separatorIndex).trim();
    const optionValue = part.slice(separatorIndex + 2).trim();

    if (!variationName || !optionValue) {
      return null;
    }

    parsedParts.push({
      variationName,
      optionValue,
      variationIndex: index,
    });
  }

  return parsedParts;
}

function toSignature(parts: ParsedVariantNamePart[]) {
  return createVariantSignature(
    parts.map((part) => ({
      variationName: part.variationName,
      optionValue: part.optionValue,
    })),
  );
}

function parseVariantEntries(
  variants: PriceFormVariantInput[],
): ParsedVariantEntry[] | null {
  const parsedEntries: ParsedVariantEntry[] = [];

  for (const variant of variants) {
    const parsedParts = parseVariantNameStrict(variant.name);

    if (!parsedParts) {
      return null;
    }

    parsedEntries.push({
      parts: parsedParts,
      signature: toSignature(parsedParts),
      variant,
    });
  }

  return parsedEntries;
}

function hasDuplicateSignatures(entries: ParsedVariantEntry[]) {
  const seen = new Set<string>();

  for (const entry of entries) {
    if (seen.has(entry.signature)) {
      return true;
    }

    seen.add(entry.signature);
  }

  return false;
}

function toVariationOptions(
  entries: ParsedVariantEntry[],
): OptionInputObject[][] | null {
  const firstEntry = entries[0];

  if (!firstEntry) {
    return null;
  }

  const variationCount = firstEntry.parts.length;

  if (variationCount < 1 || variationCount > 2) {
    return null;
  }

  const variationNames = firstEntry.parts.map((part) => part.variationName);
  const optionLists = Array.from({ length: variationCount }, () => {
    return [] as OptionInputObject[];
  });
  const optionSetByVariation = Array.from({ length: variationCount }, () => {
    return new Set<string>();
  });

  for (const entry of entries) {
    if (entry.parts.length !== variationCount) {
      return null;
    }

    for (let index = 0; index < variationCount; index += 1) {
      const currentPart = entry.parts[index];
      const expectedVariationName = variationNames[index];

      if (!currentPart || currentPart.variationName !== expectedVariationName) {
        return null;
      }

      const normalizedOptionValue = normalizeSignatureValue(
        currentPart.optionValue,
      );

      if (optionSetByVariation[index].has(normalizedOptionValue)) {
        continue;
      }

      optionSetByVariation[index].add(normalizedOptionValue);
      optionLists[index].push({
        id: `${index}:${normalizedOptionValue}`,
        value: currentPart.optionValue,
      });
    }
  }

  return optionLists;
}

function toVariations(entries: ParsedVariantEntry[]): Variation[] | null {
  const firstEntry = entries[0];

  if (!firstEntry) {
    return null;
  }

  const optionLists = toVariationOptions(entries);

  if (!optionLists) {
    return null;
  }

  return firstEntry.parts.map((part, index) => {
    return {
      id: `variation:${index}`,
      name: part.variationName,
      optionsList: optionLists[index] ?? [],
    };
  });
}

function isCreatePriceFormVariantInput(
  variant: PriceFormVariantInput,
): variant is CreatePriceFormVariantInput {
  return "image" in variant;
}

function isUpdatePriceFormVariantInput(
  variant: PriceFormVariantInput,
): variant is UpdatePriceFormVariantInput {
  return "existImage" in variant || "newImage" in variant;
}

function getVariantImageForSeed(
  variant: PriceFormVariantInput,
): PriceFormVariantImageInput | undefined {
  if (isCreatePriceFormVariantInput(variant)) {
    return variant.image;
  }

  if (isUpdatePriceFormVariantInput(variant)) {
    return variant.newImage ?? variant.existImage;
  }

  return undefined;
}

function toSeedImage(image: PriceFormVariantImageInput | undefined) {
  if (!image) {
    return {
      imageFile: undefined,
      imagePath: undefined,
      imageAlt: undefined,
    };
  }

  if ("file" in image) {
    return {
      imageFile: image.file,
      imagePath: undefined,
      imageAlt: image.alt,
    };
  }

  return {
    imageFile: undefined,
    imagePath: image.path,
    imageAlt: image.alt,
  };
}

function toDraftSeeds(entries: ParsedVariantEntry[]): VariantDraftSeed[] {
  return entries.map((entry) => {
    const image = toSeedImage(getVariantImageForSeed(entry.variant));

    return {
      variantId: "id" in entry.variant ? entry.variant.id : undefined,
      signature: entry.signature,
      sku: entry.variant.sku,
      costPrice: entry.variant.costPrice,
      sellingPrice: entry.variant.sellingPrice,
      stock: entry.variant.stock.quantity,
      imageFile: image.imageFile,
      imagePath: image.imagePath,
      imageAlt: image.imageAlt,
    };
  });
}

export function parseExistingVariantsForEditor(
  variants: PriceFormVariantInput[],
): ParsedVariantEditorResult {
  if (variants.length < 1) {
    return {
      success: false,
      error: "Existing variants are required to initialize variation editor.",
    };
  }

  const parsedEntries = parseVariantEntries(variants);

  if (!parsedEntries) {
    return {
      success: false,
      error:
        "Cannot parse existing variant names. Expected format: Variation: Option / Variation2: Option2",
    };
  }

  if (hasDuplicateSignatures(parsedEntries)) {
    return {
      success: false,
      error:
        "Duplicate variant combinations detected. Each variation combination must be unique.",
    };
  }

  const variations = toVariations(parsedEntries);

  if (!variations || variations.length < 1) {
    return {
      success: false,
      error:
        "Existing variant names are not structurally consistent across all rows.",
    };
  }

  return {
    success: true,
    data: {
      variations,
      seeds: toDraftSeeds(parsedEntries),
    },
  };
}

export function parseExplicitVariationsForEditor(
  variations: PriceFormVariationInput[],
  variants: PriceFormVariantInput[],
): ParsedVariantEditorResult {
  if (variations.length < 1) {
    return {
      success: false,
      error: "Existing variations are required to initialize variation editor.",
    };
  }

  const normalizedVariations: Variation[] = variations.map((variation) => ({
    id: variation.id,
    name: variation.name,
    optionsList: variation.options.map((option) => ({
      id: option.id,
      value: option.value,
    })),
  }));

  const variationLookup = new Map(
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

  const seeds: VariantDraftSeed[] = [];
  const seenSignatures = new Set<string>();

  for (const variant of variants) {
    const selectedOptions = variant.selectedOptions ?? [];

    if (selectedOptions.length !== variations.length) {
      return {
        success: false,
        error:
          "Existing variant selections do not match the configured variations.",
      };
    }

    const signatureParts: VariantSignaturePart[] = [];

    for (const selection of selectedOptions) {
      const variation = variationLookup.get(selection.variationId);
      const optionValue = variation?.optionsById.get(selection.optionId);

      if (!variation || !optionValue) {
        return {
          success: false,
          error:
            "Existing variant selections do not match the configured variations.",
        };
      }

      signatureParts.push({
        variationName: variation.name,
        optionValue,
      });
    }

    const signature = createVariantSignature(signatureParts);

    if (seenSignatures.has(signature)) {
      return {
        success: false,
        error:
          "Duplicate variant combinations detected. Each variation combination must be unique.",
      };
    }

    seenSignatures.add(signature);

    const image = toSeedImage(getVariantImageForSeed(variant));

    seeds.push({
      variantId: "id" in variant ? variant.id : undefined,
      signature,
      sku: variant.sku,
      costPrice: variant.costPrice,
      sellingPrice: variant.sellingPrice,
      stock: variant.stock.quantity,
      imageFile: image.imageFile,
      imagePath: image.imagePath,
      imageAlt: image.imageAlt,
    });
  }

  return {
    success: true,
    data: {
      variations: normalizedVariations,
      seeds,
    },
  };
}

export function buildVariantName(selectedOptions: VariantOptionSelection[]) {
  return selectedOptions
    .map((selectedOption) => {
      const variationName =
        selectedOption.variationName ||
        `Variation ${selectedOption.variationIndex + 1}`;

      return `${variationName}: ${selectedOption.optionValue}`;
    })
    .join(" / ");
}

export function mapDraftRowsToFormVariants(
  rows: VariantDraftRow[],
  mode: "create" | "update",
): PriceFormVariantInput[] {
  return rows.map((row, index) => {
    const commonFields = {
      name: buildVariantName(row.selectedOptions),
      sku: row.sku.trim(),
      costPrice: row.costPrice,
      sellingPrice: row.sellingPrice,
      stock: {
        quantity: row.stock,
      },
      position: index,
      selectedOptions: row.selectedOptions.map((selectedOption) => ({
        variationId: selectedOption.variationId,
        optionId: selectedOption.optionId,
      })),
    };

    if (mode === "create") {
      return {
        clientKey: row.clientKey,
        ...commonFields,
        image: row.imageFile
          ? {
              file: row.imageFile,
              alt: row.imageAlt,
            }
          : undefined,
      };
    }

    return {
      id: row.variantId,
      ...commonFields,
      existImage: row.imagePath
        ? {
            path: row.imagePath,
            alt: row.imageAlt,
          }
        : undefined,
      newImage: row.imageFile
        ? {
            file: row.imageFile,
            alt: row.imageAlt,
          }
        : undefined,
    };
  });
}

export function getImageGroupKey(row: VariantDraftRow) {
  return row.selectedOptions[0]?.optionId ?? row.combinationKey;
}

export function toImageInputId(groupKey: string) {
  return `variant-image-${groupKey.replace(/[^a-zA-Z0-9-_]/g, "-")}`;
}
