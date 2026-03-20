"use client";

import axios from "axios";
import ProductForm from "../../../feature/products/product-form/product-form";
import { CreateProductFormData } from "@/lib/validation-schema/product-schema";

const CreateNewProductPage = () => {
  const onSubmit = async (data: CreateProductFormData) => {
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
  };

  return <ProductForm mode="create" onSubmit={onSubmit} />;
};

export default CreateNewProductPage;
