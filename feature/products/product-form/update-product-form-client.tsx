"use client";

import type { SubmitHandler } from "react-hook-form";
import type {
  UpdateProductFormData,
  UpdateProductFormInputData,
} from "@/lib/products/schema";
import ProductForm from "./product-form";

type Props = {
  product: UpdateProductFormInputData;
  onSubmit?: SubmitHandler<UpdateProductFormData>;
};

// The future API integration supplies mapped form values and a submit handler.
export default function UpdateProductFormClient({ product, onSubmit }: Props) {
  return <ProductForm mode="update" product={product} onSubmit={onSubmit} />;
}
