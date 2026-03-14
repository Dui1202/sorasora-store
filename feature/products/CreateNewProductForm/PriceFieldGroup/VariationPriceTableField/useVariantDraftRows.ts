import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  mergeDraftValues,
  VariantCombination,
  VariantDraftRow,
} from "./useVariantCombinations";
import {
  getImageGroupKey,
  NumericDraftField,
  parseNonNegativeInt,
} from "./variationPriceTableField.utils";

export function useVariantDraftRows(combinations: VariantCombination[]) {
  const [draftRowsState, setDraftRowsState] = useState<VariantDraftRow[]>([]);
  const previewUrlsRef = useRef(new Set<string>());

  const draftRows = useMemo(() => {
    return mergeDraftValues(draftRowsState, combinations);
  }, [draftRowsState, combinations]);

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
