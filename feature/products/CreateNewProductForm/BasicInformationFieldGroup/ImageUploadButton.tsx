"use client";

import { Button } from "@/components/ui/button";
import { CreateProductFormInputData } from "@/lib/validationSchema";
import { DragDropProvider } from "@dnd-kit/react";
import { ImageUp } from "lucide-react";
import { useCallback, useEffect, useRef } from "react";
import { useFormContext } from "react-hook-form";
import UploadedImagesContainer from "./UploadedImagesContainer";
import type { UploadedImagePreview } from "./type";
import { useUploadImage } from "./useUploadImage";

const ImageUploadButton = ({ draftsId }: { draftsId: string }) => {
  const inputRef = useRef<HTMLInputElement | null>(null);

  const { control, setValue, getValues, register } =
    useFormContext<CreateProductFormInputData>();
  const syncProductImagesToForm = useCallback(
    (images: UploadedImagePreview[]) => {
      const productImages = images
        .filter((img) => img.status === "done" && img.url)
        .map((img) => {
          return {
            url: img.url!,
            alt: img.fileName,
            position: img.position,
          };
        });
      setValue("productImages", productImages, {
        shouldDirty: true,
        shouldTouch: true,
        shouldValidate: true,
      });
    },
    [setValue],
  );

  const { images, upload, reorder, remove } = useUploadImage(
    draftsId,
    syncProductImagesToForm,
  );
  useEffect(() => {
    register("productImages");
  }, [register]);

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
        onChange={upload}
      />

      <DragDropProvider onDragEnd={reorder}>
        <UploadedImagesContainer images={images} remove={remove} />
      </DragDropProvider>
    </div>
  );
};

export default ImageUploadButton;
