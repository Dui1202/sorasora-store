import { Button } from "@/components/ui/button";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/productSchema";
import { Plus } from "lucide-react";
import { useState } from "react";
import { useFormContext, useWatch } from "react-hook-form";
import VariationFieldGroup from "./VariationFieldGroup/VariationFieldGroup";
import { calculateBreakEvenPrice } from "./pricing/calculateBreakEvenPrice";
import { useVariation } from "./VariationFieldGroup/useVariation";

const PriceFieldGroup = () => {
  const variationState = useVariation();
  const { addVariation, isVariationEnable } = variationState;

  const { control } = useFormContext<CreateProductFormInputData>();

  const supplierCountry = useWatch({
    control,
    name: "supplierCountry",
  });

  const [breakEvenPrice, setBreakEvenPrice] = useState(0);
  return (
    <>
      <FieldGroup>
        <Field>
          <FieldLabel>Variations</FieldLabel>
          {!isVariationEnable && (
            <div>
              <Button onClick={addVariation}>
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
                placeholder="Orginal price..."
                onChange={(event) => {
                  setBreakEvenPrice(
                    calculateBreakEvenPrice(
                      supplierCountry,
                      "NONE",
                      parseInt(event.target.value),
                    ) ?? 0,
                  );

                  console.log(
                    calculateBreakEvenPrice(
                      supplierCountry,
                      "NONE",
                      parseInt(event.target.value),
                    ),
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
              <Input placeholder="Sale price..." />
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
        <VariationFieldGroup variationState={variationState} />
      )}
    </>
  );
};

export default PriceFieldGroup;
