import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/validation-schema/product-schema";

export type FileImagePreview = {
  id: string;
  type: "file";
  displayUrl: string;
  fileName: string;
  file: File;
  alt?: string;
  position: number;
};

export type UploadedImagePreview = {
  id: string;
  type: "uploaded";
  displayUrl: string;
  fileName: string;
  path: string;
  alt?: string;
  position: number;
};

export type ImagePreview = FileImagePreview | UploadedImagePreview;
