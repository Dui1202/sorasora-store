import { useCallback, useState } from "react";
import { Variation } from "./type";
import type { OptionInputObject } from "../OptionInputGroup";

const MAX_VARIATIONS = 2;

const createEmptyVariation = (): Variation => {
  return {
    id: crypto.randomUUID(),
    optionsList: [],
  };
};

const isOptionListEqual = (
  current: OptionInputObject[],
  next: OptionInputObject[],
) => {
  if (current === next) return true;
  if (current.length !== next.length) return false;

  for (let index = 0; index < current.length; index += 1) {
    if (
      current[index].id !== next[index].id ||
      current[index].value !== next[index].value
    ) {
      return false;
    }
  }

  return true;
};

export function useVariation() {
  const [variations, setVariations] = useState<Variation[]>([]);
  const isVariationEnable = variations.length > 0;
  const isVariationLimitExceed = variations.length >= MAX_VARIATIONS;

  const addVariation = () => {
    setVariations((prev) => {
      if (prev.length >= MAX_VARIATIONS) {
        return prev;
      }

      return [...prev, createEmptyVariation()];
    });
  };

  const removeVariation = (index: number) => {
    setVariations((prev) => {
      if (!Number.isInteger(index) || index < 0 || index >= prev.length) {
        return prev;
      }

      const newVariations = [...prev];
      newVariations.splice(index, 1);
      return newVariations;
    });
  };

  const updateVariationOptions = useCallback(
    (variationId: string, next: OptionInputObject[]) => {
      setVariations((prev) => {
        const variationIndex = prev.findIndex((variation) => {
          return variation.id === variationId;
        });

        if (variationIndex < 0) {
          return prev;
        }

        const currentVariation = prev[variationIndex];

        if (isOptionListEqual(currentVariation.optionsList, next)) {
          return prev;
        }

        const updated = [...prev];
        updated[variationIndex] = {
          ...currentVariation,
          optionsList: next,
        };

        return updated;
      });
    },
    [],
  );

  return {
    variations,
    addVariation,
    removeVariation,
    isVariationEnable,
    isVariationLimitExceed,
    updateVariationOptions,
  };
}
