"use client";

import { Button } from "@/components/ui/button";
import axios from "axios";
import { ImageUp } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import UploadedImagesContainer, {
  UploadedImagePreview,
} from "./UploadedImagesContainer";

const ImageUploadButton = ({ draftsId }: { draftsId: string }) => {
  const inputRef = useRef<HTMLInputElement | null>(null);
  const previewUrlsRef = useRef<string[]>([]);
  const [images, setImages] = useState<UploadedImagePreview[]>([]);

  const handleButtonClick = () => {
    inputRef.current?.click();
  };

  useEffect(() => {
    return () => {
      for (const previewUrl of previewUrlsRef.current) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, []);

  const handleFileChange = async (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const files = Array.from(event.target.files ?? []);

    if (!files.length) {
      return;
    }

    const createdPreviews = files.map((file) => {
      const previewUrl = URL.createObjectURL(file);
      previewUrlsRef.current.push(previewUrl);

      return {
        id: crypto.randomUUID(),
        fileName: file.name,
        previewUrl,
        status: "uploading" as const,
      };
    });

    setImages((prev) => [...prev, ...createdPreviews]);

    await Promise.all(
      files.map(async (file, index) => {
        const currentPreview = createdPreviews[index];
        const formData = new FormData();
        formData.append("image", file);

        try {
          const { data } = await axios.post(
            `/api/products/drafts/${draftsId}/images`,
            formData,
          );

          setImages((prev) =>
            prev.map((image) =>
              image.id === currentPreview.id
                ? {
                    ...image,
                    status: "done",
                    path: data.path,
                    publicUrl: data.publicUrl,
                  }
                : image,
            ),
          );
        } catch (error) {
          let message = "Image upload failed.";

          if (axios.isAxiosError(error)) {
            const errorMessage = error.response?.data?.message;
            message =
              typeof errorMessage === "string" ? errorMessage : error.message;
          } else if (error instanceof Error) {
            message = error.message;
          }

          setImages((prev) =>
            prev.map((image) =>
              image.id === currentPreview.id
                ? {
                    ...image,
                    status: "error",
                    errorMessage: message,
                  }
                : image,
            ),
          );
        }
      }),
    );

    event.target.value = "";
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
        onChange={handleFileChange}
      />

      <UploadedImagesContainer images={images} />
    </div>
  );
};

export default ImageUploadButton;
