"use client";

import { Button } from "@/components/ui/button";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/productSchema";
import { DragDropProvider } from "@dnd-kit/react";
import { ImageUp } from "lucide-react";
import { useEffect, useRef } from "react";
import { useFormContext } from "react-hook-form";
import UploadedImagesContainer from "./UploadedImagesContainer";
import { usePreviewImage } from "./usePreviewImage";

const ImageUploadButton = () => {
  const inputRef = useRef<HTMLInputElement | null>(null);

  const { setValue, register } = useFormContext<CreateProductFormInputData>();

  const { images, addImages, reorder, remove } = usePreviewImage();

  useEffect(() => {
    register("productImages");
  }, [register]);

  useEffect(() => {
    setValue(
      "productImages",
      images.map((img) => ({
        file: img.file,
        alt: img.fileName,
        position: img.position,
      })),
      {
        shouldDirty: true,
        shouldTouch: true,
        shouldValidate: true,
      },
    );
  }, [images, setValue]);

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
        <UploadedImagesContainer images={images} remove={remove} />
      </DragDropProvider>
    </div>
  );
};

export default ImageUploadButton;
