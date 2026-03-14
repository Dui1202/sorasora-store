import { useMemo } from "react";
import { OptionInputObject } from "../OptionInputGroup";
import { Variation } from "../VariationFieldGroup/type";

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
  selectedOptions: VariantOptionSelection[];
};

export type VariantDraftRow = VariantCombination & {
  costPrice: number;
  sellingPrice: number;
  sku: string;
  stock: number;
  imageFile?: File;
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

function createCombinationKey(selectedOptions: VariantOptionSelection[]) {
  return selectedOptions
    .map((selection) => {
      return `${selection.variationId}:${selection.optionId}`;
    })
    .join("|");
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

function createDefaultDraftRow(combination: VariantCombination): VariantDraftRow {
  return {
    ...combination,
    costPrice: 0,
    sellingPrice: 0,
    sku: "",
    stock: 0,
    imageFile: undefined,
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

  if (renderableVariations.some((variation) => variation.optionsList.length === 0)) {
    return [];
  }

  const [firstVariation, secondVariation] = renderableVariations;

  if (!secondVariation) {
    return firstVariation.optionsList.map((option) => {
      const selectedOptions = [createSelection(firstVariation, option, 0)];

      return {
        combinationKey: createCombinationKey(selectedOptions),
        selectedOptions,
      };
    });
  }

  const combinations: VariantCombination[] = [];

  for (const firstOption of firstVariation.optionsList) {
    for (const secondOption of secondVariation.optionsList) {
      const selectedOptions = [
        createSelection(firstVariation, firstOption, 0),
        createSelection(secondVariation, secondOption, 1),
      ];

      combinations.push({
        combinationKey: createCombinationKey(selectedOptions),
        selectedOptions,
      });
    }
  }

  return combinations;
}

export function mergeDraftValues(
  previousRows: VariantDraftRow[],
  nextCombinations: VariantCombination[],
): VariantDraftRow[] {
  const previousRowsByKey = new Map(
    previousRows.map((row) => [row.combinationKey, row]),
  );

  return nextCombinations.map((combination) => {
    const matchedRow = previousRowsByKey.get(combination.combinationKey);

    if (!matchedRow) {
      return createDefaultDraftRow(combination);
    }

    return {
      ...matchedRow,
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
