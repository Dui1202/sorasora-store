import { useEffect, useMemo } from "react";
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Variation } from "../variation-field-group/type";
import {
  createVariantTableRows,
  useVariantCombinations,
} from "./use-variant-combinations";
import {
  useVariantDraftRows,
  VariantDraftSeed,
} from "./use-variant-draft-rows";
import {
  getVariationLabel,
  mapDraftRowsToFormVariants,
} from "./variation-price-table-field-utils";
import VariantPriceTableRow from "./variant-price-table-row";
import {
  PriceFormMode,
  PriceFormVariantInput,
} from "../types";

type VariationPriceTableFieldProps = {
  mode: PriceFormMode;
  variations: Variation[];
  costCurrencyCode: string;
  initialDraftSeeds?: VariantDraftSeed[];
  onVariantsChange?: (variants: PriceFormVariantInput[]) => void;
};

const VariationPriceTableField = ({
  mode,
  variations,
  costCurrencyCode,
  initialDraftSeeds,
  onVariantsChange,
}: VariationPriceTableFieldProps) => {
  const renderableVariations = useMemo(() => {
    return variations.slice(0, 2);
  }, [variations]);

  const { combinations } = useVariantCombinations(renderableVariations);
  const { draftRows, updateNumericField, updateSku, updateVariantImage } =
    useVariantDraftRows(combinations, initialDraftSeeds);

  useEffect(() => {
    onVariantsChange?.(mapDraftRowsToFormVariants(draftRows));
  }, [draftRows, mode, onVariantsChange]);

  const tableRows = useMemo(() => {
    return createVariantTableRows(draftRows, renderableVariations.length);
  }, [draftRows, renderableVariations.length]);

  if (renderableVariations.length === 0) {
    return null;
  }

  if (combinations.length === 0) {
    return (
      <div className="rounded-md border p-4 text-sm text-muted-foreground">
        Add at least one option for each variation to generate variant rows.
      </div>
    );
  }

  const isSingleVariation = renderableVariations.length === 1;

  return (
    <div className="rounded-md border">
      <Table className="min-w-[780px]">
        <TableHeader>
          <TableRow>
            {renderableVariations.map((variation, index) => (
              <TableHead key={variation.id}>
                {getVariationLabel(variation, index)}
              </TableHead>
            ))}
            <TableHead className="min-w-40">Cost Price</TableHead>
            <TableHead className="min-w-40">Selling Price</TableHead>
            <TableHead className="min-w-40">SKU</TableHead>
            <TableHead className="min-w-32">Stock</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {tableRows.map((row) => (
            <VariantPriceTableRow
              key={row.combinationKey}
              row={row}
              costCurrencyCode={costCurrencyCode}
              isSingleVariation={isSingleVariation}
              onNumericFieldChange={updateNumericField}
              onSkuChange={updateSku}
              onImageSelected={updateVariantImage}
            />
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default VariationPriceTableField;
