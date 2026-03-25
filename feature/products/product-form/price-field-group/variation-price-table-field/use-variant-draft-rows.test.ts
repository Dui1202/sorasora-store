import { describe, expect, it } from "vitest";
import {
  mergeDraftValues,
  VariantCombination,
} from "./use-variant-combinations";
import { mergeCurrentDraftRows } from "./use-variant-draft-rows";

const combinations: VariantCombination[] = [
  {
    combinationKey: "color:red",
    signature: "color:red",
    selectedOptions: [
      {
        variationId: "variation-1",
        variationName: "Color",
        variationIndex: 0,
        optionId: "option-red",
        optionValue: "Red",
      },
    ],
  },
  {
    combinationKey: "color:blue",
    signature: "color:blue",
    selectedOptions: [
      {
        variationId: "variation-1",
        variationName: "Color",
        variationIndex: 0,
        optionId: "option-blue",
        optionValue: "Blue",
      },
    ],
  },
];

describe("mergeCurrentDraftRows", () => {
  it("uses seeded draft rows when the local draft state is still empty", () => {
    const seededDraftRows = mergeDraftValues([], combinations).map((row) => {
      if (row.signature !== "color:red") {
        return row;
      }

      return {
        ...row,
        costPrice: 125,
        sellingPrice: 180,
        sku: "RED-001",
        imagePath: "products/variants/red.png",
        imageAlt: "Red variant",
      };
    });

    const mergedRows = mergeCurrentDraftRows([], seededDraftRows, combinations);

    expect(mergedRows[0]).toMatchObject({
      signature: "color:red",
      costPrice: 125,
      sellingPrice: 180,
      sku: "RED-001",
      imagePath: "products/variants/red.png",
      imageAlt: "Red variant",
    });
    expect(mergedRows[1]).toMatchObject({
      signature: "color:blue",
      costPrice: 0,
      sellingPrice: 0,
      sku: "",
    });
  });
});
