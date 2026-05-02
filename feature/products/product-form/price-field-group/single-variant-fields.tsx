import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/components/ui/input-group";
import { type ChangeEvent } from "react";
import { type UseFormRegisterReturn } from "react-hook-form";
import {
  formatDigitsWithPeriodSeparator,
  handleDigitOnlyKeyDown,
  handleDigitOnlyPaste,
} from "./number-guard";
import { getFormattedPrice } from "./pricing/calculate-break-even-price";

/**
 * Props for the single-variant pricing form.
 *
 * The parent container owns all state and handlers. This component is kept
 * presentational so the main `PriceFieldGroup` can decide when single-variant
 * fields should be shown versus the variation editor.
 */
type SingleVariantFieldsProps = {
  baseCostPriceField: UseFormRegisterReturn<"baseCostPrice">;
  baseSellingPriceField: UseFormRegisterReturn<"baseSellingPrice">;
  baseSkuField: UseFormRegisterReturn<"baseSku">;
  baseStockField: UseFormRegisterReturn<"baseStock.quantity">;
  baseSellingPrice?: number;
  breakEvenPrice: number;
  costCurrencyCode: string;
  onBaseCostPriceChange: (event: ChangeEvent<HTMLInputElement>) => void;
  onBaseSellingPriceChange: (event: ChangeEvent<HTMLInputElement>) => void;
};

/**
 * Renders the pricing inputs for products that do not use variations.
 *
 * Once variation mode is enabled, these fields are no longer the source of
 * truth because pricing/stock moves into the generated variant table.
 */
function SingleVariantFields({
  baseCostPriceField,
  baseSellingPriceField,
  baseSkuField,
  baseStockField,
  baseSellingPrice,
  breakEvenPrice,
  costCurrencyCode,
  onBaseCostPriceChange,
  onBaseSellingPriceChange,
}: SingleVariantFieldsProps) {
  return (
    <>
      <FieldGroup className="flex flex-row">
        <Field>
          <FieldLabel>Original Price</FieldLabel>
          <FieldDescription>
            Orginal price in the supplier country&apos;s currency
          </FieldDescription>
          <InputGroup>
            <InputGroupAddon>{costCurrencyCode}</InputGroupAddon>
            <InputGroupInput
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              placeholder="Orginal price..."
              {...baseCostPriceField}
              onKeyDown={handleDigitOnlyKeyDown}
              onPaste={handleDigitOnlyPaste}
              onChange={onBaseCostPriceChange}
            />
          </InputGroup>
          <FieldDescription>
            Break-even Price:
            {" " + getFormattedPrice(breakEvenPrice)}
          </FieldDescription>
        </Field>
        <Field>
          <FieldLabel>Sale Price</FieldLabel>
          <FieldDescription>Price you want to sale for</FieldDescription>
          <InputGroup>
            <InputGroupAddon>VND</InputGroupAddon>
            {/* Keep the sale price formatted for display while still letting
                the parent normalize it back into a numeric form value. */}
            <InputGroupInput
              type="text"
              inputMode="numeric"
              pattern="[0-9]*"
              placeholder="Sale price..."
              {...baseSellingPriceField}
              value={formatDigitsWithPeriodSeparator(
                String(baseSellingPrice ?? ""),
              )}
              onKeyDown={handleDigitOnlyKeyDown}
              onPaste={handleDigitOnlyPaste}
              onChange={onBaseSellingPriceChange}
            />
          </InputGroup>
        </Field>
        <Field>
          <FieldLabel>SKU</FieldLabel>
          <FieldDescription>SKU for single-variant products</FieldDescription>
          <Input placeholder="SKU..." {...baseSkuField} />
        </Field>
      </FieldGroup>
      <FieldGroup className="grid grid-cols-2">
        <Field className="col-span-1">
          <FieldLabel>Stock</FieldLabel>
          <Input
            type="text"
            inputMode="numeric"
            pattern="[0-9]*"
            placeholder="Stock..."
            {...baseStockField}
            onKeyDown={handleDigitOnlyKeyDown}
            onPaste={handleDigitOnlyPaste}
            onChange={baseStockField.onChange}
          />
        </Field>
      </FieldGroup>
    </>
  );
}

export default SingleVariantFields;
