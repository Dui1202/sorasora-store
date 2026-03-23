"use client";

import ProductForm from "@/feature/products/product-form/product-form";
import { useProduct } from "@/hooks/use-products";
import { toUpdateProductCommandInput } from "@/lib/validation-schema/product-schema/product-command";
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
      const multipartFormData = new FormData();

      multipartFormData.append(
        "payload",
        JSON.stringify({
          ...data,
          variants: data.variants.map((variant) => ({
            ...variant,
            existImage: variant.existImage
              ? {
                  path: variant.existImage.path,
                  alt: variant.existImage.alt,
                }
              : undefined,
            newImage: variant.newImage
              ? {
                  alt: variant.newImage.alt,
                }
              : undefined,
          })),
          existProductImages: data.existProductImages.map((image) => ({
            path: image.path,
            alt: image.alt,
            position: image.position,
          })),
          newProductImages: data.newProductImages.map((image) => ({
            alt: image.alt,
            position: image.position,
          })),
        }),
      );

      for (const image of data.newProductImages) {
        multipartFormData.append("images", image.file);
      }

      for (const variant of data.variants) {
        if (variant.newImage) {
          multipartFormData.append("variantImages", variant.newImage.file);
        }
      }

      await axios.patch(`/api/products/${id}`, multipartFormData);
    };
  }, [id]);

  if (!id || !productInput) {
    return null;
  }

  return <ProductForm mode="update" product={productInput} onSubmit={onSubmit} />;
};

export default EditProductPage;
