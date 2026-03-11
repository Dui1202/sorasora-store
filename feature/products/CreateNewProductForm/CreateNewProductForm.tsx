"use client";

import SectionCard from "@/components/SectionCard";
import { FieldLegend, FieldSet } from "@/components/ui/field";
import { Separator } from "@/components/ui/separator";
import { PriceFieldGroup } from "@/feature/products/CreateNewProductForm/PriceFieldGroup";
import { SpecificationFieldGroup } from "@/feature/products/CreateNewProductForm/SpecificationFieldGroup";
import {
  CreateProductFormData,
  CreateProductFormInputData,
  ProductImages,
  UploadProductImageResponse,
  createProductFormSchema,
  toCreateProductCommand,
} from "@/lib/validation-schema/product-schema/validationSchema";
import { zodResolver } from "@hookform/resolvers/zod";
import { Heading } from "@radix-ui/themes";
import axios from "axios";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import FormButton from "../../../app/products/_components/FormButton";
import BasicInformationFieldGroup from "./BasicInformationFieldGroup/BasicInformationFieldGroup";

const CreateNewProductForm = () => {
  const methods = useForm<
    CreateProductFormInputData,
    unknown,
    CreateProductFormData
  >({
    resolver: zodResolver(createProductFormSchema),
  });

  const { handleSubmit } = methods;

  const submitProduct: SubmitHandler<CreateProductFormData> = async (data) => {
    const uploadedProductImages: ProductImages = [];

    try {
      for (const image of data.productImages) {
        const formData = new FormData();
        formData.append("file", image.file);

        const { data: uploadResult } =
          await axios.post<UploadProductImageResponse>(
            "/api/uploads/product-images",
            formData,
          );

        uploadedProductImages.push({
          path: uploadResult.path,
          alt: image.alt,
          position: image.position,
        });
      }

      const payload = toCreateProductCommand(data, uploadedProductImages);
      await axios.post("/api/products", payload);
      console.log(payload);
    } catch (error) {
      await cleanUpProductImages(uploadedProductImages);
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
        <FormButton />
      </form>
    </FormProvider>
  );
};

export default CreateNewProductForm;
