import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/validation-schema/product-schema";

export type PriceFormMode = "create" | "update";
export type PriceFormInputData =
  | CreateProductFormInputData
  | UpdateProductFormInputData;
export type PriceFormVariantInput = NonNullable<
  PriceFormInputData["variants"]
>[number];
export type PriceFormVariantImageInput = NonNullable<
  PriceFormVariantInput["image"]
>;
