import { Field, FieldLabel } from "@/components/ui/field";
import { InputGroup, InputGroupInput } from "@/components/ui/input-group";
import { OptionInputGroup } from "../OptionInputGroup";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus, X } from "lucide-react";
import { useCallback } from "react";
import { Variation } from "./type";
import { useVariation } from "./useVariation";

type VariationFieldGroupProps = {
  variationState: ReturnType<typeof useVariation>;
};

const VariationFieldGroup = ({
  variationState: {
    removeVariation,
    addVariation,
    variations,
    isVariationLimitExceed,
    updateVariationOptions,
    updateVariationName,
  },
}: VariationFieldGroupProps) => {
  return (
    <div className="gap-4 flex flex-col">
      {variations.map((variation, index) => (
        <VariationField
          variation={variation}
          index={index}
          key={variation.id}
          onCloseButtonClicked={() => {
            removeVariation(index);
          }}
          updateVariationOptions={updateVariationOptions}
          updateVariationName={updateVariationName}
        />
      ))}

      {!isVariationLimitExceed && (
        <Button variant="secondary" onClick={addVariation} type="button">
          <Plus />
          Add Variation
        </Button>
      )}
    </div>
  );
};

type VariationFieldProps = {
  index: number;
  variation: Variation;
  onCloseButtonClicked: () => void;
  updateVariationOptions: (id: string, next: Variation["optionsList"]) => void;
  updateVariationName: (id: string, next: string) => void;
};

const VariationField = ({
  variation,
  index,
  onCloseButtonClicked,
  updateVariationOptions,
  updateVariationName,
}: VariationFieldProps) => {
  const handleOptionsListChange = useCallback(
    (next: Variation["optionsList"]) => {
      updateVariationOptions(variation.id, next);
    },
    [updateVariationOptions, variation.id],
  );

  return (
    <Card className="relative flex flex-col gap-4 p-5 pt-3">
      <div className="absolute right-1 top-1">
        <Button
          size="icon"
          variant="ghost"
          type="button"
          onClick={onCloseButtonClicked}
        >
          <X />
        </Button>
      </div>
      <Field>
        <FieldLabel className="text-xl">Variation {index + 1}</FieldLabel>
        <InputGroup>
          <InputGroupInput
            placeholder=""
            onChange={(value) => {
              updateVariationName(variation.id, value.target.value);
            }}
          />
        </InputGroup>
      </Field>
      <Field>
        <FieldLabel>Options</FieldLabel>
        <OptionInputGroup
          initialOptionsList={variation.optionsList}
          onListChange={handleOptionsListChange}
        />
      </Field>
    </Card>
  );
};

export default VariationFieldGroup;
