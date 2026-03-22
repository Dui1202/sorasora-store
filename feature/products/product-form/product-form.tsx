"use client";

import SectionCard from "@/components/section-card";
import { FieldLegend, FieldSet } from "@/components/ui/field";
import { Separator } from "@/components/ui/separator";
import { PriceFieldGroup } from "@/feature/products/product-form/price-field-group";
import { SpecificationFieldGroup } from "@/feature/products/product-form/specification-field-group";

import { zodResolver } from "@hookform/resolvers/zod";
import { Heading } from "@radix-ui/themes";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import FormButton from "../../../app/products/_components/form-button";
import BasicInformationFieldGroup from "./basic-information-field-group/basic-information-field-group";
import {
  CreateProductFormData,
  CreateProductFormInputData,
  createProductFormSchema,
  UpdateProductFormData,
  UpdateProductFormInputData,
  updateProductFormSchema,
} from "@/lib/validation-schema/product-schema";

type CreateProductFormProps = {
  mode: "create";
  onSubmit: SubmitHandler<CreateProductFormData>;
  product?: never;
};

type UpdateProductFormProps = {
  mode: "update";
  onSubmit: SubmitHandler<UpdateProductFormData>;
  product: UpdateProductFormInputData;
};

type ProductFormProps = CreateProductFormProps | UpdateProductFormProps;

const ProductForm = ({ mode, onSubmit, product }: ProductFormProps) => {
  if (mode === "create") {
    return <CreateModeForm onSubmit={onSubmit} mode="create" />;
  }

  return <UpdateModeForm onSubmit={onSubmit} product={product} mode="update" />;
};

const CreateModeForm = ({ onSubmit }: CreateProductFormProps) => {
  const methods = useForm<
    CreateProductFormInputData,
    unknown,
    CreateProductFormData
  >({
    resolver: zodResolver(createProductFormSchema),
  });

  const { handleSubmit } = methods;
  const { isSubmitting } = methods.formState;

  return (
    <FormProvider {...methods}>
      <form className="p-5" onSubmit={handleSubmit(onSubmit)}>
        <Heading>Add Product</Heading>
        <Separator className="my-2" />
        <ProductFormFields mode="create" />
        <FormButton isSubmitting={isSubmitting} />
      </form>
    </FormProvider>
  );
};

const UpdateModeForm = ({ onSubmit, product }: UpdateProductFormProps) => {
  const methods = useForm<
    UpdateProductFormInputData,
    unknown,
    UpdateProductFormData
  >({
    resolver: zodResolver(updateProductFormSchema),
    defaultValues: product,
  });

  const { handleSubmit } = methods;
  const { isSubmitting } = methods.formState;

  return (
    <FormProvider {...methods}>
      <form className="p-5" onSubmit={handleSubmit(onSubmit)}>
        <Heading>Edit Product</Heading>
        <Separator className="my-2" />
        <ProductFormFields mode="update" />
        <FormButton isSubmitting={isSubmitting} />
      </form>
    </FormProvider>
  );
};

const ProductFormFields = ({ mode }: { mode: "create" | "update" }) => {
  return (
    <>
      <SectionCard>
        <FieldSet>
          <FieldLegend>Basic Information</FieldLegend>
          <BasicInformationFieldGroup />
        </FieldSet>
      </SectionCard>
      <SectionCard>
        <FieldSet>
          <FieldLegend>Specification</FieldLegend>
          <SpecificationFieldGroup />
        </FieldSet>
      </SectionCard>
      <SectionCard>
        <FieldSet>
          <FieldLegend>Sales Information</FieldLegend>
          <PriceFieldGroup mode={mode} />
        </FieldSet>
      </SectionCard>
    </>
  );
};

export default ProductForm;
