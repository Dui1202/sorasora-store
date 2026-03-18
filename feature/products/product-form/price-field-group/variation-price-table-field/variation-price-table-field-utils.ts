import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema";
import { Variation } from "../variation-field-group/type";
import {
  VariantDraftRow,
  VariantOptionSelection,
} from "./use-variant-combinations";

export type NumericDraftField = "costPrice" | "sellingPrice" | "stock";

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
): CreateProductFormInputData["variants"] {
  return rows.map((row, index) => {
    return {
      name: buildVariantName(row.selectedOptions),
      sku: row.sku.trim(),
      costPrice: row.costPrice,
      sellingPrice: row.sellingPrice,
      position: index,
      image: row.imageFile
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
