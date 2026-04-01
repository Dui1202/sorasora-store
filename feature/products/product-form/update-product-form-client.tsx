"use client";

import {
  toUpdateProductCommandInput,
  toUpdateProductMultipartFormData,
} from "@/lib/products/mappers/product-command.mapper";
import {
  ProductWithRelations,
  UpdateProductFormData,
} from "@/lib/validation-schema/product-schema";
import axios from "axios";
import { useMemo } from "react";
import ProductForm from "./product-form";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { routes } from "@/lib/routes";

type Props = {
  product: ProductWithRelations;
};

const UpdateProductFormClient = ({ product }: Props) => {
  const id = product.id;
  const productInput = toUpdateProductCommandInput(product);
  const router = useRouter();

  const onSubmit = async (data: UpdateProductFormData) => {
    const multipartFormData = toUpdateProductMultipartFormData(data);

    await axios.patch(`/api/products/${id}`, multipartFormData);

    toast.success("Product updated successfully.");
    router.push(routes.admin.products);
  };

  return (
    <ProductForm mode="update" onSubmit={onSubmit} product={productInput} />
  );
};

export default UpdateProductFormClient;
