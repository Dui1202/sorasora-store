import { useEffect, useMemo } from "react";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/productSchema";
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Variation } from "../VariationFieldGroup/type";
import {
  createVariantTableRows,
  useVariantCombinations,
} from "./useVariantCombinations";
import { useVariantDraftRows } from "./useVariantDraftRows";
import {
  getVariationLabel,
  mapDraftRowsToFormVariants,
} from "./variationPriceTableField.utils";
import VariantPriceTableRow from "./VariantPriceTableRow";

type VariationPriceTableFieldProps = {
  variations: Variation[];
  onVariantsChange?: (variants: CreateProductFormInputData["variants"]) => void;
};

const VariationPriceTableField = ({
  variations,
  onVariantsChange,
}: VariationPriceTableFieldProps) => {
  const renderableVariations = useMemo(() => {
    return variations.slice(0, 2);
  }, [variations]);

  const { combinations } = useVariantCombinations(renderableVariations);
  const { draftRows, updateNumericField, updateSku, updateVariantImage } =
    useVariantDraftRows(combinations);

  useEffect(() => {
    onVariantsChange?.(mapDraftRowsToFormVariants(draftRows));
  }, [draftRows, onVariantsChange]);

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
