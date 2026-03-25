"use client";

import ProductForm from "@/feature/products/product-form/product-form";
import { useProduct } from "@/hooks/use-products";
import {
  toUpdateProductCommandInput,
  toUpdateProductMultipartFormData,
} from "@/lib/products/mappers/product-command.mapper";
import { UpdateProductFormData } from "@/lib/validation-schema/product-schema";
import axios from "axios";
import { useParams } from "next/navigation";
import { useMemo } from "react";

const EditProductPage = () => {
  const { id } = useParams<{ id: string }>();
  const { data: product } = useProduct(id);
  const productInput = useMemo(() => {
    if (!product) {
      return null;
    }

    return toUpdateProductCommandInput(product);
  }, [product]);

  const onSubmit = useMemo(() => {
    return async (data: UpdateProductFormData) => {
      const multipartFormData = toUpdateProductMultipartFormData(data);
      await axios.patch(`/api/products/${id}`, multipartFormData);
    };
  }, [id]);

  if (!id || !productInput) {
    return null;
  }

  return (
    <ProductForm mode="update" product={productInput} onSubmit={onSubmit} />
  );
};

export default EditProductPage;
