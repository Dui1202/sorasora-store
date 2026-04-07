import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/products/schema";

export type PriceFormMode = "create" | "update";
export type PriceFormInputData =
  | CreateProductFormInputData
  | UpdateProductFormInputData;
export type CreatePriceFormVariantInput = NonNullable<
  CreateProductFormInputData["variants"]
>[number];
export type UpdatePriceFormVariantInput = NonNullable<
  UpdateProductFormInputData["variants"]
>[number];
export type PriceFormVariantInput =
  | CreatePriceFormVariantInput
  | UpdatePriceFormVariantInput;
export type PriceFormVariantImageInput =
  | NonNullable<CreatePriceFormVariantInput["image"]>
  | NonNullable<UpdatePriceFormVariantInput["existImage"]>
  | NonNullable<UpdatePriceFormVariantInput["newImage"]>;
