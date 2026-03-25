import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Input } from "@/components/ui/input";
import { TableCell, TableRow } from "@/components/ui/table";
import {
  handleDigitOnlyKeyDown,
  handleDigitOnlyPaste,
  toDigitsOnly,
} from "../number-guard";
import VariantImageInput from "./variant-image-input";
import { VariantTableRow } from "./use-variant-combinations";
import { NumericDraftField } from "./variation-price-table-field-utils";

type VariantPriceTableRowProps = {
  row: VariantTableRow;
  costCurrencyCode: string;
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
  costCurrencyCode,
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
        <InputGroup>
          <InputGroupAddon>{costCurrencyCode}</InputGroupAddon>
          <InputGroupInput
            type="text"
            inputMode="numeric"
            pattern="[0-9]*"
            value={row.costPrice}
            onKeyDown={handleDigitOnlyKeyDown}
            onPaste={handleDigitOnlyPaste}
            onChange={(event) => {
              onNumericFieldChange(
                row.combinationKey,
                "costPrice",
                toDigitsOnly(event.target.value),
              );
            }}
            placeholder="0"
          />
        </InputGroup>
      </TableCell>

      <TableCell>
        <InputGroup>
          <InputGroupAddon>VND</InputGroupAddon>
          <InputGroupInput
            type="text"
            inputMode="numeric"
            pattern="[0-9]*"
            value={row.sellingPrice}
            onKeyDown={handleDigitOnlyKeyDown}
            onPaste={handleDigitOnlyPaste}
            onChange={(event) => {
              onNumericFieldChange(
                row.combinationKey,
                "sellingPrice",
                toDigitsOnly(event.target.value),
              );
            }}
            placeholder="0"
          />
        </InputGroup>
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
          type="text"
          inputMode="numeric"
          pattern="[0-9]*"
          value={row.stock}
          onKeyDown={handleDigitOnlyKeyDown}
          onPaste={handleDigitOnlyPaste}
          onChange={(event) => {
            onNumericFieldChange(
              row.combinationKey,
              "stock",
              toDigitsOnly(event.target.value),
            );
          }}
          placeholder="0"
        />
      </TableCell>
    </TableRow>
  );
};

export default VariantPriceTableRow;
