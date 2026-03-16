import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";
import useOptionInputList from "@/feature/products/create-new-product-form/price-field-group/option-input-group/use-option-input-list";
import { DragDropProvider } from "@dnd-kit/react";
import { useSortable } from "@dnd-kit/react/sortable";
import { Move, Trash2 } from "lucide-react";
import type { OptionInputObject } from "./types";
import { useEffect } from "react";

const ARIA_LABEL_MOVE = "Move input";
const ARIA_LABEL_REMOVE = "Remove input";

type OptionInputGroupProps = {
  initialOptionsList?: OptionInputObject[];
  onListChange?: (next: OptionInputObject[]) => void;
};

const OptionInputGroup = ({
  initialOptionsList,
  onListChange,
}: OptionInputGroupProps) => {
  const { handleRemove, optionsInputs, handleChange, reorder } =
    useOptionInputList(initialOptionsList);

  useEffect(() => {
    onListChange?.(optionsInputs.filter((x) => x.value.trim() !== ""));
  }, [optionsInputs, onListChange]);

  return (
    <DragDropProvider onDragEnd={reorder}>
      <div className="grid grid-cols-2 gap-2">
        {optionsInputs.map((item, index) => {
          const disabledActions =
            index === optionsInputs.length - 1 && item.value.trim() === "";

          return (
            <OptionInputRow
              key={item.id}
              item={item}
              index={index}
              handleChange={handleChange}
              handleRemove={handleRemove}
              disabledActions={disabledActions}
            />
          );
        })}
      </div>
    </DragDropProvider>
  );
};

type OptionsInputProps = {
  item: OptionInputObject;
  index: number;
  handleChange: (value: string, id: string) => void;
  handleRemove: (id: string) => void;
  disabledActions: boolean;
};

const OptionInputRow = ({
  item,
  index,
  handleChange,
  handleRemove,
  disabledActions,
}: OptionsInputProps) => {
  const { ref, handleRef } = useSortable({
    id: item.id,
    index,
    group: "options-list",
    disabled: disabledActions,
  });

  return (
    <div ref={ref}>
      <InputGroup className="col-span-1">
        <InputGroupInput
          placeholder={"Options " + (index + 1)}
          value={item.value}
          onChange={(e) => handleChange(e.target.value, item.id)}
        />
        <InputGroupAddon align="inline-end">
          <InputGroupButton
            ref={handleRef}
            aria-label={ARIA_LABEL_MOVE}
            disabled={disabledActions}
          >
            <Move />
          </InputGroupButton>
          <InputGroupButton
            onClick={() => handleRemove(item.id)}
            aria-label={ARIA_LABEL_REMOVE}
            disabled={disabledActions}
          >
            <Trash2 />
          </InputGroupButton>
        </InputGroupAddon>
      </InputGroup>
    </div>
  );
};

export default OptionInputGroup;
