import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { InputGroup, InputGroupInput } from "@/components/ui/input-group";
import { Plus } from "lucide-react";
import { useState } from "react";
import OptionInputGroup from "@/feature/products/CreateNewProductForm/OptionInputGroup/OptionInputGroup";
import { SupplierCountry } from "@/app/generated/prisma/enums";
import { calculateBreakEvenPrice } from "./pricing/calculateBreakEvenPrice";
import { useFormContext, useWatch } from "react-hook-form";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/validationSchema";

const PriceFieldGroup = () => {
  const [isVariationEnable, setVariationEnable] = useState(false);

  const onEnableVariationButtonClick = () => {
    setVariationEnable(true);
  };

  const { control } = useFormContext<CreateProductFormInputData>();

  const supplierCountry = useWatch({
    control,
    name: "supplierCountry",
  });

  const [breakEvenPrice, setBreakEvenPrice] = useState(0);
  const originalPrice = useWatch({
    control,
    name: "baseOriginalPrice",
  });
  return (
    <>
      <FieldGroup>
        <Field>
          <FieldLabel>Variations</FieldLabel>
          {!isVariationEnable && (
            <div>
              <Button onClick={onEnableVariationButtonClick}>
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
                Orginal price in the supplier country's currency
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
        <Card className="grid-cols-2 p-5">
          <Field>
            <FieldLabel>Variation 1</FieldLabel>
            <InputGroup>
              <InputGroupInput placeholder="" />
            </InputGroup>
          </Field>
          <Field>
            <FieldLabel>Options</FieldLabel>
            <OptionInputGroup />
          </Field>
        </Card>
      )}
    </>
  );
};

export default PriceFieldGroup;
