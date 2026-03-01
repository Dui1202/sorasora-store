"use client";

import SectionCard from "@/components/SectionCard";
import { FieldLegend, FieldSet } from "@/components/ui/field";
import { Separator } from "@/components/ui/separator";
import {
  CreateProductFormInputData,
  CreateProductFormOutputData,
  createProductFormSchema,
} from "@/lib/validationSchema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Heading } from "@radix-ui/themes";
import axios from "axios";
import { useMemo } from "react";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import BasicInformationFieldGroup from "./BasicInformationFieldGroup";
import FormButton from "./FormButton";
import PriceFieldGroup from "./PriceFieldGroup";
import SpecificationFieldGroup from "./SpecificationFieldGroup";
import StockFieldGroup from "./StockFieldGroup";
import VariationsFieldGroup from "./VariationsFieldGroup";
import { DragDropProvider } from "@dnd-kit/react";

const CreateNewProductForm = () => {
  const draftsId = useMemo(() => crypto.randomUUID(), []);
  const methods = useForm<
    CreateProductFormInputData,
    unknown,
    CreateProductFormOutputData
  >({
    resolver: zodResolver(createProductFormSchema),
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isLoading, isSubmitting },
  } = methods;

  const submitProduct: SubmitHandler<CreateProductFormOutputData> = async (
    data,
  ) => {
    try {
      await axios.post("/api/products", data);
      console.log(data);
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
