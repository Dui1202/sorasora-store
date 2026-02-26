import {
  ComboboxContent,
  Combobox,
  ComboboxInput,
  ComboboxEmpty,
  ComboboxList,
  ComboboxItem,
} from "@/components/ui/combobox";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { SelectContent, SelectLabel } from "@/components/ui/select";
import {
  SelectTrigger,
  SelectValue,
  SelectGroup,
  SelectItem,
} from "@/components/ui/select";
import { register } from "module";
import { Select } from "@/components/ui/select";
import React from "react";

const SpecificationFieldGroup = () => {
  const franchises = ["Genshin Impact", "Arknights"];

  return (
    <FieldGroup className="flex flex-row">
      <Field>
        <FieldLabel>Supplier Country</FieldLabel>
        <Select>
          <SelectTrigger>
            <SelectValue placeholder="Choose a country..." />
          </SelectTrigger>
          <SelectContent>
            <SelectGroup>
              <SelectLabel>Country</SelectLabel>
              <SelectItem value={"JAPAN"}>Japan</SelectItem>
              <SelectItem value={"CHINA"}>China</SelectItem>
            </SelectGroup>
          </SelectContent>
        </Select>
      </Field>
      <Field>
        <FieldLabel>Franchise</FieldLabel>
        <Combobox items={franchises}>
          <ComboboxInput placeholder="Select a franchise" />
          <ComboboxContent>
            <ComboboxEmpty>No franchise found</ComboboxEmpty>
            <ComboboxList>
              {(item) => (
                <ComboboxItem key={item} value={item}>
                  {item}
                </ComboboxItem>
              )}
            </ComboboxList>
          </ComboboxContent>
        </Combobox>
      </Field>
    </FieldGroup>
  );
};

export default SpecificationFieldGroup;
