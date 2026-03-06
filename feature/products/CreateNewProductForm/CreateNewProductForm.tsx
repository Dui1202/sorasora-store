"use client";

import SectionCard from "@/components/SectionCard";
import { FieldLegend, FieldSet } from "@/components/ui/field";
import { Separator } from "@/components/ui/separator";
import { PriceFieldGroup } from "@/feature/products/CreateNewProductForm/PriceFieldGroup";
import { SpecificationFieldGroup } from "@/feature/products/CreateNewProductForm/SpecificationFieldGroup";
import {
  CreateProductFormData,
  CreateProductFormInputData,
  createProductFormSchema,
  toCreateProductCommand,
} from "@/lib/validationSchema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Heading } from "@radix-ui/themes";
import axios from "axios";
import { useMemo } from "react";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import BasicInformationFieldGroup from "./BasicInformationFieldGroup/BasicInformationFieldGroup";
import FormButton from "../../../app/products/_components/FormButton";

const CreateNewProductForm = () => {
  const draftsId = useMemo(() => crypto.randomUUID(), []);
  const methods = useForm<
    CreateProductFormInputData,
    unknown,
    CreateProductFormData
  >({
    resolver: zodResolver(createProductFormSchema),
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isLoading, isSubmitting },
  } = methods;

  const submitProduct: SubmitHandler<CreateProductFormData> = async (data) => {
    try {
      const payload = toCreateProductCommand(data);
      await axios.post("/api/products", payload);
      console.log(payload);
    } catch (error) {
      console.log(error);
    }
  };
  return (
    <FormProvider {...methods}>
      <form className="p-5" onSubmit={handleSubmit(submitProduct)}>
        <Heading>Add Product</Heading>
        <Separator className="my-2" />
        <SectionCard>
          <FieldSet>
            <FieldLegend>Basic Information</FieldLegend>
            <BasicInformationFieldGroup draftsId={draftsId} />
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

            <PriceFieldGroup />
          </FieldSet>
        </SectionCard>
        <FormButton draftsId={draftsId} />
      </form>
    </FormProvider>
  );
};

export default CreateNewProductForm;
