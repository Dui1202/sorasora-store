import { useMemo } from "react";
import { OptionInputObject } from "../option-input-group";
import { Variation } from "../variation-field-group/type";

const MAX_RENDERED_VARIATIONS = 2;

export type VariantOptionSelection = {
  variationId: string;
  variationName: string;
  variationIndex: number;
  optionId: string;
  optionValue: string;
};

export type VariantCombination = {
  combinationKey: string;
  signature: string;
  selectedOptions: VariantOptionSelection[];
};

export type VariantDraftRow = VariantCombination & {
  clientKey: string;
  costPrice: number;
  sellingPrice: number;
  sku: string;
  stock: number;
  imageFile?: File;
  imagePath?: string;
  imageAlt?: string;
  imagePreviewUrl?: string;
};

export type VariantTableRow = VariantDraftRow & {
  showFirstVariationCell: boolean;
  firstVariationRowSpan: number;
};

function normalizeVariationOptions(optionsList: OptionInputObject[]) {
  return optionsList.filter((option) => {
    return option.value.trim() !== "";
  });
}

export type VariantSignaturePart = {
  variationName: string;
  optionValue: string;
};

export function normalizeSignatureValue(value: string) {
  return value.trim().toLowerCase();
}

export function createVariantSignature(selectedOptions: VariantSignaturePart[]) {
  return selectedOptions
    .map((selection) => {
      return `${normalizeSignatureValue(selection.variationName)}:${normalizeSignatureValue(selection.optionValue)}`;
    })
    .join("|");
}

function createCombination(
  selectedOptions: VariantOptionSelection[],
): VariantCombination {
  const signature = createVariantSignature(
    selectedOptions.map((selection) => ({
      variationName: selection.variationName,
      optionValue: selection.optionValue,
    })),
  );

  return {
    combinationKey: signature,
    signature,
    selectedOptions,
  };
}

function createSelection(
  variation: Variation,
  option: OptionInputObject,
  variationIndex: number,
): VariantOptionSelection {
  return {
    variationId: variation.id,
    variationName: variation.name.trim(),
    variationIndex,
    optionId: option.id,
    optionValue: option.value.trim(),
  };
}

function toRenderableVariations(variations: Variation[]) {
  return variations.slice(0, MAX_RENDERED_VARIATIONS).map((variation) => {
    return {
      ...variation,
      optionsList: normalizeVariationOptions(variation.optionsList),
    };
  });
}

function createDefaultDraftRow(
  combination: VariantCombination,
): VariantDraftRow {
  return {
    ...combination,
    clientKey: crypto.randomUUID(),
    costPrice: 0,
    sellingPrice: 0,
    sku: "",
    stock: 0,
    imageFile: undefined,
    imagePath: undefined,
    imageAlt: undefined,
    imagePreviewUrl: undefined,
  };
}

export function createVariantCombinations(
  variations: Variation[],
): VariantCombination[] {
  const renderableVariations = toRenderableVariations(variations);

  if (renderableVariations.length === 0) {
    return [];
  }

  if (
    renderableVariations.some((variation) => variation.optionsList.length === 0)
  ) {
    return [];
  }

  const [firstVariation, secondVariation] = renderableVariations;

  if (!secondVariation) {
    return firstVariation.optionsList.map((option) => {
      const selectedOptions = [createSelection(firstVariation, option, 0)];

      return createCombination(selectedOptions);
    });
  }

  const combinations: VariantCombination[] = [];

  for (const firstOption of firstVariation.optionsList) {
    for (const secondOption of secondVariation.optionsList) {
      const selectedOptions = [
        createSelection(firstVariation, firstOption, 0),
        createSelection(secondVariation, secondOption, 1),
      ];

      combinations.push(createCombination(selectedOptions));
    }
  }

  return combinations;
}

export function mergeDraftValues(
  previousRows: VariantDraftRow[],
  nextCombinations: VariantCombination[],
): VariantDraftRow[] {
  const previousRowsBySignature = new Map(
    previousRows.map((row) => [row.signature, row]),
  );

  return nextCombinations.map((combination) => {
    const matchedRow = previousRowsBySignature.get(combination.signature);

    if (!matchedRow) {
      return createDefaultDraftRow(combination);
    }

    return {
      ...matchedRow,
      combinationKey: combination.combinationKey,
      signature: combination.signature,
      selectedOptions: combination.selectedOptions,
    };
  });
}

export function createVariantTableRows(
  draftRows: VariantDraftRow[],
  variationCount: number,
): VariantTableRow[] {
  if (variationCount < 2) {
    return draftRows.map((row) => ({
      ...row,
      showFirstVariationCell: true,
      firstVariationRowSpan: 1,
    }));
  }

  const firstVariationGroupSize = new Map<string, number>();

  for (const row of draftRows) {
    const firstSelection = row.selectedOptions[0];

    if (!firstSelection) {
      continue;
    }

    firstVariationGroupSize.set(
      firstSelection.optionId,
      (firstVariationGroupSize.get(firstSelection.optionId) ?? 0) + 1,
    );
  }

  const renderedGroups = new Set<string>();

  return draftRows.map((row) => {
    const firstSelection = row.selectedOptions[0];
    const groupId = firstSelection?.optionId ?? row.combinationKey;
    const showFirstVariationCell = !renderedGroups.has(groupId);

    if (showFirstVariationCell) {
      renderedGroups.add(groupId);
    }

    return {
      ...row,
      showFirstVariationCell,
      firstVariationRowSpan: showFirstVariationCell
        ? (firstVariationGroupSize.get(groupId) ?? 1)
        : 1,
    };
  });
}

export function useVariantCombinations(variations: Variation[]) {
  const combinations = useMemo(() => {
    return createVariantCombinations(variations);
  }, [variations]);

  return {
    combinations,
  };
}
