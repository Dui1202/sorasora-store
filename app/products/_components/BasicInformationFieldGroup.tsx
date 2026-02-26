import { FieldGroup } from "@/components/ui/field";
import { Textarea } from "@/components/ui/textarea";
import { Field, FieldLabel } from "@/components/ui/field";
import { register } from "module";
import React from "react";
import ImageUploadButton from "./ImageUploadButton";
import { Input } from "@/components/ui/input";

const BasicInformationFieldGroup = ({ draftsId }: { draftsId: string }) => {
  return (
    <FieldGroup>
      <Field>
        <FieldLabel>Product Images</FieldLabel>
        <ImageUploadButton draftsId={draftsId} />
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
  );
};

export default BasicInformationFieldGroup;
