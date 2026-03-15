import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/product.base";
import { Plus } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import { useFormContext, useWatch } from "react-hook-form";
import VariationFieldGroup from "./VariationFieldGroup/VariationFieldGroup";
import { calculateBreakEvenPrice } from "./pricing/calculateBreakEvenPrice";
import { useVariation } from "./VariationFieldGroup/useVariation";
import VariationPriceTableField from "./VariationPriceTableField/VariationPriceTableField";

const PriceFieldGroup = () => {
  const variationState = useVariation();
  const { addVariation, isVariationEnable, variations } = variationState;

  const { control, setValue, register } =
    useFormContext<CreateProductFormInputData>();

  const supplierCountry = useWatch({
    control,
    name: "supplierCountry",
  });

  const baseCostPriceField = register("baseCostPrice", {
    valueAsNumber: true,
  });

  const baseSellingPriceField = register("baseSellingPrice", {
    valueAsNumber: true,
  });

  useEffect(() => {
    setValue("hasVariation", isVariationEnable, {
      shouldDirty: true,
    });

    if (!isVariationEnable) {
      setValue("variants", [], {
        shouldDirty: true,
      });
    }
  }, [isVariationEnable, setValue]);

  const handleVariantsChange = useCallback(
    (variants: CreateProductFormInputData["variants"]) => {
      setValue("variants", variants, {
        shouldDirty: true,
      });
    },
    [setValue],
  );

  const [breakEvenPrice, setBreakEvenPrice] = useState(0);

  return (
    <>
      <FieldGroup>
        <Field>
          <FieldLabel>Variations</FieldLabel>
          {!isVariationEnable && (
            <div>
              <Button onClick={addVariation} type="button">
                <Plus />
                Enable Variation
              </Button>
            </div>
          )}
        </Field>
      </FieldGroup>
      {!isVariationEnable && (
        <>
          <FieldGroup className="flex flex-row">
            <Field>
              <FieldLabel>Original Price</FieldLabel>
              <FieldDescription>
                Orginal price in the supplier country&apos;s currency
              </FieldDescription>
              <Input
                type="number"
                min={0}
                placeholder="Orginal price..."
                {...baseCostPriceField}
                onChange={(event) => {
                  baseCostPriceField.onChange(event);

                  setBreakEvenPrice(
                    calculateBreakEvenPrice(
                      supplierCountry,
                      "NONE",
                      parseInt(event.target.value || "0", 10),
                    ) ?? 0,
                  );
                }}
              />
              <FieldDescription>
                Break-even Price:
                {breakEvenPrice}
              </FieldDescription>
            </Field>
            <Field>
              <FieldLabel>Sale Price</FieldLabel>
              <FieldDescription>Price you want to sale for</FieldDescription>
              <Input
                type="number"
                min={0}
                placeholder="Sale price..."
                {...baseSellingPriceField}
              />
            </Field>
            <Field>
              <FieldLabel>SKU</FieldLabel>
              <FieldDescription>
                SKU for single-variant products
              </FieldDescription>
              <Input placeholder="SKU..." {...register("baseSku")} />
            </Field>
          </FieldGroup>
          <FieldGroup className="grid grid-cols-2">
            <Field className="col-span-1">
              <FieldLabel>Stock</FieldLabel>
              <Input placeholder="Stock..." />
            </Field>
          </FieldGroup>
        </>
      )}

      {isVariationEnable && (
        <div className="flex flex-col gap-4">
          <VariationFieldGroup variationState={variationState} />
          <VariationPriceTableField
            variations={variations}
            onVariantsChange={handleVariantsChange}
          />
        </div>
      )}
    </>
  );
};

export default PriceFieldGroup;
