import { Input } from "@/components/ui/input";
import { TableCell, TableRow } from "@/components/ui/table";
import VariantImageInput from "./variant-image-input";
import { VariantTableRow } from "./use-variant-combinations";
import { NumericDraftField } from "./variation-price-table-field-utils";

type VariantPriceTableRowProps = {
  row: VariantTableRow;
  isSingleVariation: boolean;
  onNumericFieldChange: (
    combinationKey: string,
    field: NumericDraftField,
    value: string,
  ) => void;
  onSkuChange: (combinationKey: string, sku: string) => void;
  onImageSelected: (groupKey: string, file: File) => void;
};

const VariantPriceTableRow = ({
  row,
  isSingleVariation,
  onNumericFieldChange,
  onSkuChange,
  onImageSelected,
}: VariantPriceTableRowProps) => {
  const [firstOption, secondOption] = row.selectedOptions;

  return (
    <TableRow key={row.combinationKey}>
      {isSingleVariation && (
        <TableCell>
          <div className="flex flex-col gap-3">
            <p>{firstOption?.optionValue}</p>
            <VariantImageInput row={row} onImageSelected={onImageSelected} />
          </div>
        </TableCell>
      )}

      {!isSingleVariation && row.showFirstVariationCell && (
        <TableCell rowSpan={row.firstVariationRowSpan} className="align-top">
          <div className="flex flex-col gap-3">
            <p>{firstOption?.optionValue}</p>
            <VariantImageInput row={row} onImageSelected={onImageSelected} />
          </div>
        </TableCell>
      )}

      {!isSingleVariation && <TableCell>{secondOption?.optionValue}</TableCell>}

      <TableCell>
        <Input
          type="number"
          inputMode="numeric"
          min={0}
          value={row.costPrice}
          onChange={(event) => {
            onNumericFieldChange(
              row.combinationKey,
              "costPrice",
              event.target.value,
            );
          }}
          placeholder="0"
        />
      </TableCell>

      <TableCell>
        <Input
          type="number"
          inputMode="numeric"
          min={0}
          value={row.sellingPrice}
          onChange={(event) => {
            onNumericFieldChange(
              row.combinationKey,
              "sellingPrice",
              event.target.value,
            );
          }}
          placeholder="0"
        />
      </TableCell>

      <TableCell>
        <Input
          value={row.sku}
          onChange={(event) => {
            onSkuChange(row.combinationKey, event.target.value);
          }}
          onBlur={(event) => {
            onSkuChange(row.combinationKey, event.target.value.trim());
          }}
          placeholder="Enter SKU"
        />
      </TableCell>

      <TableCell>
        <Input
          type="number"
          inputMode="numeric"
          min={0}
          value={row.stock}
          onChange={(event) => {
            onNumericFieldChange(
              row.combinationKey,
              "stock",
              event.target.value,
            );
          }}
          placeholder="0"
        />
      </TableCell>
    </TableRow>
  );
};

export default VariantPriceTableRow;
