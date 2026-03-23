"use client";

import axios from "axios";
import ProductForm from "../../../feature/products/product-form/product-form";
import { CreateProductFormData } from "@/lib/validation-schema/product-schema";
import { toCreateProductMultipartFormData } from "@/lib/products";

const CreateNewProductPage = () => {
  const onSubmit = async (data: CreateProductFormData) => {
    const multipartFormData = toCreateProductMultipartFormData(data);

    await axios.post("/api/products", multipartFormData);
  };

  return <ProductForm mode="create" onSubmit={onSubmit} />;
};

export default CreateNewProductPage;
