"use client";

import SectionCard from "@/app/components/SectionCard";
import { Card } from "@/components/ui/card";
import {
  Combobox,
  ComboboxContent,
  ComboboxEmpty,
  ComboboxInput,
  ComboboxItem,
  ComboboxList,
} from "@/components/ui/combobox";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Separator } from "@/components/ui/separator";
import { Textarea } from "@/components/ui/textarea";
import { Heading } from "@radix-ui/themes";
import { useForm } from "react-hook-form";

const CreateProductForm = () => {
  const {
    register,
    handleSubmit,
    formState: { errors, isLoading, isSubmitting },
  } = useForm();

  const franchises = ["Genshin Impact", "Arknights"];
  return (
    <form className="p-5">
      <Heading>Add Product</Heading>
      <Separator className="my-2" />
      <SectionCard>
        <FieldSet>
          <FieldLegend>Basic Information</FieldLegend>
          <FieldGroup>
            <Field>
              <FieldLabel>Product Images</FieldLabel>
            </Field>
            <Field>
              <FieldLabel>Product Name</FieldLabel>
              <Input placeholder="Product Name..." />
            </Field>
            <Field>
              <FieldLabel>Description</FieldLabel>
              <Textarea placeholder="Description..." />
            </Field>
          </FieldGroup>
        </FieldSet>
      </SectionCard>
      <SectionCard>
        <FieldSet>
          <FieldLegend>Specification</FieldLegend>
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
                    {(item) => <ComboboxItem>{item}</ComboboxItem>}
                  </ComboboxList>
                </ComboboxContent>
              </Combobox>
            </Field>
          </FieldGroup>
        </FieldSet>
      </SectionCard>
      <SectionCard>
        <FieldSet>
          <FieldLegend>Sales Information</FieldLegend>
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
        </FieldSet>
      </SectionCard>
    </form>
  );
};

export default CreateProductForm;
