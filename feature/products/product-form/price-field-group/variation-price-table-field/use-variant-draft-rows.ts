import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  mergeDraftValues,
  VariantCombination,
  VariantDraftRow,
} from "./use-variant-combinations";
import {
  getImageGroupKey,
  NumericDraftField,
  parseNonNegativeInt,
} from "./variation-price-table-field-utils";

export type VariantDraftSeed = {
  signature: string;
  costPrice: number;
  sellingPrice: number;
  sku: string;
  stock?: number;
  imageFile?: File;
  imagePath?: string;
  imageAlt?: string;
  imagePreviewUrl?: string;
};

export function useVariantDraftRows(
  combinations: VariantCombination[],
  initialDraftSeeds: VariantDraftSeed[] = [],
) {
  const [draftRowsState, setDraftRowsState] = useState<VariantDraftRow[]>([]);
  const previewUrlsRef = useRef(new Set<string>());

  const seededDraftRows = useMemo(() => {
    if (initialDraftSeeds.length === 0 || combinations.length === 0) {
      return [] as VariantDraftRow[];
    }

    const seedMapBySignature = new Map(
      initialDraftSeeds.map((seed) => [seed.signature, seed]),
    );
    const defaultRows = mergeDraftValues([], combinations);

    return defaultRows.map((row) => {
      const seed = seedMapBySignature.get(row.signature);

      if (!seed) {
        return row;
      }

      return {
        ...row,
        costPrice: seed.costPrice,
        sellingPrice: seed.sellingPrice,
        sku: seed.sku,
        stock: seed.stock ?? 0,
        imageFile: seed.imageFile,
        imagePath: seed.imagePath,
        imageAlt: seed.imageAlt,
        imagePreviewUrl: seed.imagePreviewUrl,
      };
    });
  }, [combinations, initialDraftSeeds]);

  const draftRows = useMemo(() => {
    const sourceRows =
      draftRowsState.length > 0 ? draftRowsState : seededDraftRows;
    return mergeDraftValues(sourceRows, combinations);
  }, [combinations, draftRowsState, seededDraftRows]);

  useEffect(() => {
    const nextPreviewUrls = new Set<string>();

    for (const row of draftRows) {
      if (row.imagePreviewUrl) {
        nextPreviewUrls.add(row.imagePreviewUrl);
      }
    }

    for (const previousPreviewUrl of previewUrlsRef.current) {
      if (!nextPreviewUrls.has(previousPreviewUrl)) {
        URL.revokeObjectURL(previousPreviewUrl);
      }
    }

    previewUrlsRef.current = nextPreviewUrls;
  }, [draftRows]);

  useEffect(() => {
    return () => {
      for (const previewUrl of previewUrlsRef.current) {
        URL.revokeObjectURL(previewUrl);
      }

      previewUrlsRef.current.clear();
    };
  }, []);

  const updateNumericField = useCallback(
    (combinationKey: string, field: NumericDraftField, rawValue: string) => {
      const nextValue = parseNonNegativeInt(rawValue);

      setDraftRowsState((previousRows) => {
        return mergeDraftValues(previousRows, combinations).map((row) => {
          if (row.combinationKey !== combinationKey) {
            return row;
          }

          return {
            ...row,
            [field]: nextValue,
          };
        });
      });
    },
    [combinations],
  );

  const updateSku = useCallback(
    (combinationKey: string, sku: string) => {
      setDraftRowsState((previousRows) => {
        return mergeDraftValues(previousRows, combinations).map((row) => {
          if (row.combinationKey !== combinationKey) {
            return row;
          }

          return {
            ...row,
            sku,
          };
        });
      });
    },
    [combinations],
  );

  const updateVariantImage = useCallback(
    (groupKey: string, file: File) => {
      const previewUrl = URL.createObjectURL(file);

      setDraftRowsState((previousRows) => {
        return mergeDraftValues(previousRows, combinations).map((row) => {
          if (getImageGroupKey(row) !== groupKey) {
            return row;
          }

          return {
            ...row,
            imageFile: file,
            imagePath: undefined,
            imageAlt: file.name,
            imagePreviewUrl: previewUrl,
          };
        });
      });
    },
    [combinations],
  );

  return {
    draftRows,
    updateNumericField,
    updateSku,
    updateVariantImage,
  };
}
