"use client";

import ProductForm from "@/feature/products/product-form/product-form";
import { useProduct } from "@/hooks/use-products";
import { toUpdateProductCommandInput } from "@/lib/validation-schema/product-schema/product-command";
import {
  UpdateProductFormData,
} from "@/lib/validation-schema/product-schema";
import axios from "axios";
import { useParams } from "next/navigation";
import { useMemo } from "react";

function isLocalProductImage(
  image: UpdateProductFormData["productImages"][number],
): image is Extract<
  UpdateProductFormData["productImages"][number],
  { file: File }
> {
  return "file" in image;
}

function isLocalVariantImage(
  image: NonNullable<UpdateProductFormData["variants"][number]["image"]>,
): image is Extract<
  NonNullable<UpdateProductFormData["variants"][number]["image"]>,
  { file: File }
> {
  return "file" in image;
}

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
            image: variant.image
              ? isLocalVariantImage(variant.image)
                ? { alt: variant.image.alt }
                : { path: variant.image.path, alt: variant.image.alt }
              : undefined,
          })),
          productImages: data.productImages.map((image) =>
            isLocalProductImage(image)
              ? {
                  alt: image.alt,
                  position: image.position,
                }
              : {
                  path: image.path,
                  alt: image.alt,
                  position: image.position,
                },
          ),
        }),
      );

      for (const image of data.productImages) {
        if (isLocalProductImage(image)) {
          multipartFormData.append("images", image.file);
        }
      }

      for (const variant of data.variants) {
        if (variant.image && isLocalVariantImage(variant.image)) {
          multipartFormData.append("variantImages", variant.image.file);
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
