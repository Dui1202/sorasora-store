import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import ImageUploadButton from "./image-upload-button";
import { useFormContext } from "react-hook-form";
import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/products/schema";

type BasicInformationFieldGroupProps = {
  mode: "create" | "update";
};

const BasicInformationFieldGroup = ({
  mode,
}: BasicInformationFieldGroupProps) => {
  const { register } = useFormContext<
    CreateProductFormInputData | UpdateProductFormInputData
  >();
  return (
    <FieldGroup>
      <Field>
        <FieldLabel>Product Images</FieldLabel>
        <ImageUploadButton mode={mode} />
      </Field>
      <Field>
        <FieldLabel>Product Name</FieldLabel>
        <Input placeholder="Product Name..." {...register("name")} />
      </Field>
      <Field>
        <FieldLabel>Description</FieldLabel>
        <Textarea placeholder="Description..." {...register("description")} />
      </Field>
    </FieldGroup>
  );
};

export default BasicInformationFieldGroup;
