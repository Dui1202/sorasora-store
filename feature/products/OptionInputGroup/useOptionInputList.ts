import { useEffect, useState } from "react";
import type { OptionInputObject } from "./types";
import { isSortable } from "@dnd-kit/react/sortable";
import { move } from "@dnd-kit/helpers";
import { DragEndEvent } from "@dnd-kit/react";

const createEmpty = (): OptionInputObject => {
  return { id: crypto.randomUUID(), value: "" };
};
function normalize(items: OptionInputObject[]): OptionInputObject[] {
  const nonEmpty = items.filter((i) => i.value.trim() !== "");
  return [...nonEmpty, createEmpty()];
}

export default function useOptionInputList(
  initial: OptionInputObject[] = [createEmpty()],
) {
  const [optionsInputs, setOptionInputs] = useState<OptionInputObject[]>(() =>
    normalize(initial),
  );
  const handleChange = (value: string, id: string) => {
    setOptionInputs((prev) => {
      const updated = [...prev];
      const index = updated.findIndex((value) => {
        return value.id === id;
      });

      if (index < 0) {
        return prev;
      }

      updated[index] = { ...updated[index], value: value };

      if (value.trim() !== "" && index === prev.length - 1) {
        updated.push(createEmpty());
      }

      return normalize(updated);
    });
  };

  const handleRemove = (id: string) => {
    setOptionInputs((prev) => {
      if (prev.length <= 1) return prev;
      const index = prev.findIndex((value) => {
        return value.id === id;
      });

      if (index < 0) return prev;

      const updated = [...prev];

      updated.splice(index, 1);

      return normalize(updated);
    });
  };
  const reorder: DragEndEvent = (event) => {
    if (event.canceled) return;
    const { source } = event.operation;

    if (isSortable(source)) {
      const { initialIndex, index } = source;

      if (initialIndex !== index) {
        setOptionInputs((items) => normalize(move(items, event)));
      }
    }
    console.table();
  };

  useEffect(() => {
    console.table(
      optionsInputs.map((item, index) => ({
        index,
        id: item.id,
        value: item.value,
        isEmpty: item.value.trim() === "",
        isTrailingPlaceholder:
          index === optionsInputs.length - 1 && item.value.trim() === "",
      })),
    );
  }, [optionsInputs]);

  return {
    handleChange,
    handleRemove,
    reorder,
    optionsInputs,
  };
}
