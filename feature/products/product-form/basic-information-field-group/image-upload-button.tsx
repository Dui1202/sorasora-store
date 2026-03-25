"use client";

import { Button } from "@/components/ui/button";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/validation-schema/product-schema";
import { DragDropProvider } from "@dnd-kit/react";
import { ImageUp } from "lucide-react";
import { useEffect, useMemo, useRef } from "react";
import { useFormContext, useWatch } from "react-hook-form";
import { UploadedImagePreview } from "./type";
import UploadedImagesContainer from "./uploaded-images-container";
import { usePreviewImage } from "./use-preview-image";

type ProductFormInputData =
  | CreateProductFormInputData
  | UpdateProductFormInputData;

type ImageUploadButtonProps = {
  mode: "create" | "update";
};

const EMPTY_UPLOADED_IMAGES: UploadedImagePreview[] = [];

const ImageUploadButton = ({ mode }: ImageUploadButtonProps) => {
  const inputRef = useRef<HTMLInputElement | null>(null);
  const isInitializedRef = useRef(false);
  const { control, setValue, register } = useFormContext<ProductFormInputData>();
  const formValues = useWatch({ control });

  const existingProductImages = useMemo(() => {
    if (mode !== "update" || !("existProductImages" in formValues)) {
      return [];
    }

    return (formValues.existProductImages ?? []).flatMap((image) => {
      if (!image?.path) {
        return [];
      }

      return [
        {
          path: image.path,
          alt: image.alt,
          position: typeof image.position === "number" ? image.position : 0,
        },
      ];
    });
  }, [formValues, mode]);

  const initialImages = useMemo<UploadedImagePreview[]>(
    () => {
      if (mode !== "update") {
        return EMPTY_UPLOADED_IMAGES;
      }

      return (existingProductImages ?? [])
        .map((image) => ({
          id: image.path,
          type: "uploaded" as const,
          displayUrl: getProductImagePublicUrl(image.path),
          fileName: image.alt || "",
          path: image.path,
          alt: image.alt,
          position: image.position,
        }))
        .sort((a, b) => a.position - b.position);
    },
    [existingProductImages, mode],
  );

  const { previewImages, addImages, reorder, remove } =
    usePreviewImage(initialImages);

  useEffect(() => {
    if (mode === "create") {
      register("productImages");
      return;
    }

    register("existProductImages");
    register("newProductImages");
  }, [mode, register]);

  useEffect(() => {
    if (previewImages.length < 1 && !isInitializedRef.current) {
      return;
    }

    const isFirstSync = !isInitializedRef.current;
    isInitializedRef.current = true;

    if (mode === "create") {
      setValue(
        "productImages",
        previewImages
          .filter(
            (
              image,
            ): image is Extract<typeof image, { type: "file"; file: File }> =>
              image.type === "file",
          )
          .map((image) => ({
            file: image.file,
            alt: image.alt ?? image.fileName,
            position: image.position,
          })),
        {
          shouldDirty: !isFirstSync,
          shouldTouch: true,
          shouldValidate: true,
        },
      );

      return;
    }

    setValue(
      "existProductImages",
      previewImages
        .filter(
          (
            image,
          ): image is Extract<typeof image, { type: "uploaded"; path: string }> =>
            image.type === "uploaded",
        )
        .map((image) => ({
          path: image.path,
          alt: image.alt ?? image.fileName,
          position: image.position,
        })),
      {
        shouldDirty: !isFirstSync,
        shouldTouch: true,
        shouldValidate: true,
      },
    );

    setValue(
      "newProductImages",
      previewImages
        .filter(
          (
            image,
          ): image is Extract<typeof image, { type: "file"; file: File }> =>
            image.type === "file",
        )
        .map((image) => ({
          file: image.file,
          alt: image.alt ?? image.fileName,
          position: image.position,
        })),
      {
        shouldDirty: !isFirstSync,
        shouldTouch: true,
        shouldValidate: true,
      },
    );
  }, [mode, previewImages, setValue]);

  const handleButtonClick = () => {
    inputRef.current?.click();
  };

  return (
    <div className="space-y-3">
      <Button type="button" onClick={handleButtonClick} size="default">
        <ImageUp />
        Upload Images
      </Button>

      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        multiple
        hidden
        onChange={addImages}
      />

      <DragDropProvider onDragEnd={reorder}>
        <UploadedImagesContainer images={previewImages} remove={remove} />
      </DragDropProvider>
    </div>
  );
};

export default ImageUploadButton;
