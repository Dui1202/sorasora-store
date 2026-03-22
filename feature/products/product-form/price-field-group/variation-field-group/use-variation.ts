import { useCallback, useState } from "react";
import { Variation } from "./type";
import type { OptionInputObject } from "../option-input-group";

const MAX_VARIATIONS = 2;

export const createEmptyVariation = (): Variation => {
  return {
    id: crypto.randomUUID(),
    name: "",
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

export function useVariation(initialVariations: Variation[] = []) {
  const [variations, setVariations] = useState<Variation[]>(initialVariations);
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

  const updateVariationName = useCallback(
    (variationId: string, name: string) => {
      setVariations((prev) => {
        const variationIndex = prev.findIndex((variation) => {
          return variation.id === variationId;
        });

        if (variationIndex < 0) {
          return prev;
        }

        const currentVariation = prev[variationIndex];

        if (currentVariation.name === name) {
          return prev;
        }

        const updated = [...prev];
        updated[variationIndex] = {
          ...currentVariation,
          name,
        };

        return updated;
      });
    },
    [],
  );

  const replaceVariations = useCallback((next: Variation[]) => {
    setVariations(next.slice(0, MAX_VARIATIONS));
  }, []);

  return {
    variations,
    addVariation,
    removeVariation,
    isVariationEnable,
    isVariationLimitExceed,
    updateVariationOptions,
    updateVariationName,
    replaceVariations,
  };
}
