import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
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
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Move, Plus, Trash2 } from "lucide-react";
import { useMemo, useState } from "react";
import { useDraggable } from "@dnd-kit/react";
import OptionsInput from "../../../feature/products/OptionInputGroup/OptionInputGroup";

const PriceFieldGroup = () => {
  const [isVariationEnable, setVariationEnable] = useState(false);

  const onEnableVariationButtonClick = () => {
    setVariationEnable(true);
  };
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
              <Input placeholder="Orginal price..." />
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
            <OptionsInput />
          </Field>
        </Card>
      )}
    </>
  );
};

export default PriceFieldGroup;
