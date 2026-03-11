import { useEffect, useState } from "react";
import type { OptionInputObject } from "./types";
import { isSortable } from "@dnd-kit/react/sortable";
import { DragEndEvent } from "@dnd-kit/react";

/**
 * Creates an empty OptionInputObject.
 * @returns An OptionInputObject with a newly generated UUID and an empty string value.
 */
const createEmpty = (): OptionInputObject => {
  return { id: crypto.randomUUID(), value: "" };
};

/**
 * Normalize an array of OptionInputObjects by removing empty ones and adding a new one if the array is not empty.
 * @param items The array of OptionInputObjects to normalize.
 * @returns The normalized array of OptionInputObjects.
 */
function normalize(items: OptionInputObject[]): OptionInputObject[] {
  const nonEmpty = items.filter((i) => i.value.trim() !== "");
  return [...nonEmpty, createEmpty()];
}

/**
 * Hook that returns an array of OptionInputObjects and functions to handle changes to it.
 * The hook normalizes the array by removing empty ones and adding a new one if the array is not empty.
 * The hook also provides a handleChange function to update the value of an OptionInputObject,
 * a handleRemove function to remove an OptionInputObject, and a reorder function to reorder the array when an item is dragged.
 * @param initial The initial array of OptionInputObjects.
 * @returns An object containing the array of OptionInputObjects, the handleChange function, the handleRemove function, and the reorder function.
 */
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
        setOptionInputs((items) => {
          if (items.length <= 1) {
            return items;
          }

          const trailingPlaceholder = items[items.length - 1];
          const sortableItems = items.slice(0, -1);

          if (
            initialIndex < 0 ||
            index < 0 ||
            initialIndex >= sortableItems.length ||
            index >= sortableItems.length
          ) {
            return items;
          }

          const reorderedItems = [...sortableItems];
          const [removed] = reorderedItems.splice(initialIndex, 1);
          reorderedItems.splice(index, 0, removed);

          return [...reorderedItems, trailingPlaceholder];
        });
      }
    }
  };

  //Debug

  useEffect(() => {
    if (process.env.NODE_ENV === "production") return;

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
