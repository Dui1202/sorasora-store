"use client";

import { Button } from "@/components/ui/button";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
  UpdateProductImage,
} from "@/lib/validation-schema/product-schema";
import { DragDropProvider } from "@dnd-kit/react";
import { ImageUp } from "lucide-react";
import { useEffect, useMemo, useRef } from "react";
import { useFormContext } from "react-hook-form";
import { UploadedImagePreview } from "./type";
import UploadedImagesContainer from "./uploaded-images-container";
import { usePreviewImage } from "./use-preview-image";

// type ProductFormInputData =
//   | CreateProductFormInputData
//   | UpdateProductFormInputData;

const ImageUploadButton = () => {
  const inputRef = useRef<HTMLInputElement | null>(null);
  const isInitializedRef = useRef(false);
  const { watch, setValue, register } =
    useFormContext<UpdateProductFormInputData>();

  const initialProductImages = watch("productImages") as UpdateProductImage[];

  function isExistingImage(
    img: UpdateProductImage,
  ): img is { path: string; position: number; alt?: string } {
    return "path" in img;
  }
  const initialImages: UploadedImagePreview[] = useMemo<UploadedImagePreview[]>(
    () =>
      initialProductImages
        .filter(isExistingImage)
        .map<UploadedImagePreview>((img) => {
          return {
            id: img.path,
            type: "uploaded",
            displayUrl: getProductImagePublicUrl(img.path),
            fileName: img.alt || "",
            path: img.path,
            alt: img.alt,
            position: img.position,
          };
        })
        .sort((a, b) => a.position - b.position),
    [initialProductImages],
  );

  const { previewImages, addImages, reorder, remove } =
    usePreviewImage(initialImages);

  useEffect(() => {
    register("productImages");
  }, [register]);

  // sync images
  useEffect(() => {
    if (previewImages.length < 1 && !isInitializedRef.current) {
      return;
    }

    const isFirstSync = !isInitializedRef.current;
    isInitializedRef.current = true;

    setValue(
      "productImages",
      previewImages.map((img) =>
        img.type === "file"
          ? {
              file: img.file,
              alt: img.alt ?? img.fileName,
              position: img.position,
            }
          : {
              path: img.path,
              alt: img.alt ?? img.fileName,
              position: img.position,
            },
      ),
      {
        shouldDirty: !isFirstSync,
        shouldTouch: true,
        shouldValidate: true,
      },
    );
  }, [previewImages, setValue]);

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
