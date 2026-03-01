import { useState } from "react";
import { OptionInputObject } from "./types";

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
  const [optionsInputs, setOptionInputs] =
    useState<OptionInputObject[]>(initial);
  const onOptionsInputChange = (value: string, id: string) => {
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

  const onRemoveButtonClick = (id: string) => {
    if (optionsInputs.length !== 1) {
      setOptionInputs((prev) => {
        const updated = [...prev];
        const index = updated.findIndex((value) => {
          return value.id === id;
        });

        if (index < 0) {
          return prev;
        }

        updated.splice(index, 1);

        return normalize(updated);
      });
    }
  };
  return {
    onOptionsInputChange,
    onRemoveButtonClick,
    setOptionInputs,
    optionsInputs,
  };
}
