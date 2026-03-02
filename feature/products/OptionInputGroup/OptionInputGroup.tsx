import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";
import useOptionInputList from "@/feature/products/OptionInputGroup/useOptionInputList";
import { DragDropProvider } from "@dnd-kit/react";
import { useSortable } from "@dnd-kit/react/sortable";
import { Move, Trash2 } from "lucide-react";
import type { OptionInputObject } from "./types";

const ARIA_LABEL_MOVE = "Move input";
const ARIA_LABEL_REMOVE = "Remove input";

const OptionInputGroup = () => {
  const { handleRemove, optionsInputs, handleChange, reorder } =
    useOptionInputList();

  return (
    <DragDropProvider onDragEnd={reorder}>
      <div className="grid grid-cols-2 gap-2">
        {optionsInputs.map((item, index) => {
          const disabledActions =
            index === optionsInputs.length - 1 && item.value.trim() === "";

          return (
            <OptionsInput
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

const OptionsInput = ({
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
  });

  return (
    <div ref={disabledActions ? null : ref}>
      <InputGroup className="col-span-1">
        <InputGroupInput
          placeholder={"Options " + (index + 1)}
          value={item.value}
          onChange={(e) => handleChange(e.target.value, item.id)}
        />
        <InputGroupAddon align="inline-end">
          <InputGroupButton
            ref={disabledActions ? null : handleRef}
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
