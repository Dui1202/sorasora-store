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
  const { data: product, error } = useProduct(id);
  const productInputResult = useMemo(() => {
    if (!product) {
      return {
        productInput: null,
        errorMessage: null,
      };
    }

    try {
      return {
        productInput: toUpdateProductCommandInput(product),
        errorMessage: null,
      };
    } catch (mapperError) {
      return {
        productInput: null,
        errorMessage:
          mapperError instanceof Error
            ? mapperError.message
            : "Failed to load product inventory state.",
      };
    }
  }, [product]);

  const onSubmit = useMemo(() => {
    return async (data: UpdateProductFormData) => {
      const multipartFormData = toUpdateProductMultipartFormData(data);
      await axios.patch(`/api/products/${id}`, multipartFormData);
    };
  }, [id]);

  if (!id) {
    return null;
  }

  if (error) {
    return (
      <p className="p-5 text-sm text-destructive">Failed to load product.</p>
    );
  }

  if (productInputResult.errorMessage) {
    return (
      <p className="p-5 text-sm text-destructive">
        {productInputResult.errorMessage}
      </p>
    );
  }

  if (!productInputResult.productInput) {
    return null;
  }

  return (
    <ProductForm
      mode="update"
      product={productInputResult.productInput}
      onSubmit={onSubmit}
    />
  );
};

export default EditProductPage;
