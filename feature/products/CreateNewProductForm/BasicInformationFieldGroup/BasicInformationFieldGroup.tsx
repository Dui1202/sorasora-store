import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import ImageUploadButton from "./ImageUploadButton";
import { useFormContext } from "react-hook-form";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/validationSchema";

const BasicInformationFieldGroup = () => {
  const { register } = useFormContext<CreateProductFormInputData>();
  return (
    <FieldGroup>
      <Field>
        <FieldLabel>Product Images</FieldLabel>
        <ImageUploadButton />
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
