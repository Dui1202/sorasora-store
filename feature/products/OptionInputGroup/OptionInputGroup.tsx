import useOptionInputList from "@/feature/products/OptionInputGroup/useOptionInputList";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";
import { move } from "@dnd-kit/helpers";
import { DragDropProvider } from "@dnd-kit/react";
import { isSortable, useSortable } from "@dnd-kit/react/sortable";
import { Move, Trash2 } from "lucide-react";
import { OptionInputObject } from "./types";
const OptionInputGroup = () => {
  const {
    onOptionsInputChange,
    optionsInputs,
    onRemoveButtonClick,
    setOptionInputs,
  } = useOptionInputList();
  return (
    <DragDropProvider
      onDragEnd={(event) => {
        if (event.canceled) return;
        const { source } = event.operation;

        if (isSortable(source)) {
          const { initialIndex, index } = source;

          if (initialIndex !== index) {
            setOptionInputs((items) => move(items, event));
          }
        }
      }}
    >
      <div className="grid grid-cols-2 gap-2">
        {optionsInputs.map((item, index) => {
          return (
            <OptionsInput
              item={item}
              index={index}
              handleChange={onOptionsInputChange}
              handleRemove={onRemoveButtonClick}
              key={item.id}
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
};

const OptionsInput = ({
  item,
  index,
  handleChange,
  handleRemove,
}: OptionsInputProps) => {
  const { ref, handleRef } = useSortable({
    id: item.id,
    index,
    group: "options-list",
  });

  return (
    <div ref={ref}>
      <InputGroup className="col-span-1">
        <InputGroupInput
          placeholder={"Options " + (index + 1)}
          value={item.value}
          onChange={(e) => {
            handleChange(e.target.value, item.id);
          }}
        />
        <InputGroupAddon align="inline-end">
          <InputGroupButton ref={handleRef}>
            <Move />
          </InputGroupButton>
          <InputGroupButton onClick={() => handleRemove(item.id)}>
            <Trash2 />
          </InputGroupButton>
        </InputGroupAddon>
      </InputGroup>
    </div>
  );
};

export default OptionInputGroup;
