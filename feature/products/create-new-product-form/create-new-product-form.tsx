"use client";

import SectionCard from "@/components/section-card";
import { FieldLegend, FieldSet } from "@/components/ui/field";
import { Separator } from "@/components/ui/separator";
import { PriceFieldGroup } from "@/feature/products/create-new-product-form/price-field-group";
import { SpecificationFieldGroup } from "@/feature/products/create-new-product-form/specification-field-group";

import { zodResolver } from "@hookform/resolvers/zod";
import { Heading } from "@radix-ui/themes";
import axios from "axios";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import FormButton from "../../../app/products/_components/form-button";
import BasicInformationFieldGroup from "./basic-information-field-group/basic-information-field-group";
import {
  CreateProductFormData,
  CreateProductFormInputData,
  createProductFormSchema,
} from "@/lib/validation-schema/product-schema";

const CreateNewProductForm = () => {
  const methods = useForm<
    CreateProductFormInputData,
    unknown,
    CreateProductFormData
  >({
    resolver: zodResolver(createProductFormSchema),
  });

  const { handleSubmit } = methods;

  const { isSubmitting } = methods.formState;

  const submitProduct: SubmitHandler<CreateProductFormData> = async (data) => {
    try {
      const multipartFormData = new FormData();
      multipartFormData.append(
        "payload",
        JSON.stringify({
          ...data,
          variants: data.variants.map((variant) => ({
            ...variant,
            image: variant.image
              ? {
                  alt: variant.image.alt,
                }
              : undefined,
          })),
          productImages: data.productImages.map((image) => ({
            alt: image.alt,
            position: image.position,
          })),
        }),
      );

      for (const image of data.productImages) {
        multipartFormData.append("images", image.file);
      }

      for (const variant of data.variants) {
        if (variant.image?.file) {
          multipartFormData.append("variantImages", variant.image.file);
        }
      }

      await axios.post("/api/products", multipartFormData);
    } catch (error) {
      console.error("Failed to create product", error);
      throw error;
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

            <PriceFieldGroup />
          </FieldSet>
        </SectionCard>
        <FormButton isSubmitting={isSubmitting} />
      </form>
    </FormProvider>
  );
};

export default CreateNewProductForm;
