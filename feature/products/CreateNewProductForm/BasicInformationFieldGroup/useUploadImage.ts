import { useEffect, useRef, useState } from "react";
import type { UploadedImagePreview, UploadImageResponse } from "./type";
import axios from "axios";
import { set } from "zod";
import { DragEndEvent } from "@dnd-kit/react";
import { isSortable } from "@dnd-kit/react/sortable";
import { move } from "@dnd-kit/helpers";

export function useUploadImage(
  draftsId: string,
  syncImagesToForm: (images: UploadedImagePreview[]) => void,
) {
  const [images, setImages] = useState<UploadedImagePreview[]>([]);
  const previewUrlsRef = useRef(new Map<string, string>());

  useEffect(() => {
    syncImagesToForm(images);
  }, [images, syncImagesToForm]);

  const upload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    ///Create preview URLs for the selected files and add them to the state
    const files = Array.from(event.target.files || []);
    const createdPreviews: UploadedImagePreview[] = files.map((file, index) => {
      const id = crypto.randomUUID();
      const previewUrl = URL.createObjectURL(file);
      previewUrlsRef.current.set(id, previewUrl);
      return {
        id,
        fileName: file.name,
        previewUrl,
        status: "uploading" as const,
        position: images.length + index, // Position based on current images count and index in the new files
      };
    });

    setImages((prev) => [...prev, ...createdPreviews]);

    //Upload each images to the backend and update the state based on the result
    await Promise.all(
      files.map(async (file, index) => {
        const preview = createdPreviews[index];
        const formData = new FormData();
        formData.append("file", file);

        try {
          const { data } = await axios.post<UploadImageResponse>(
            `/api/products/drafts/${draftsId}/images`,
            formData,
          );
          setImages((prev) => {
            const updated = prev.map((img) =>
              preview.id === img.id
                ? {
                    ...img,
                    url: data.publicUrl,
                    status: "done" as const,
                  }
                : img,
            );

            return updated;
          });
        } catch (error) {
          setImages((prev) => {
            return prev.map((img) =>
              preview.id === img.id
                ? {
                    ...img,
                    status: "error",
                    errorMessage:
                      error instanceof Error
                        ? error.message
                        : "An unknown error occurred",
                  }
                : img,
            );
          });
        }
      }),
    );

    event.target.value = "";
  };

  const remove = (id: string) => {
    setImages((prev) => {
      const updated = prev.filter((img) => img.id !== id);
      return updated;
    });

    const previewUrl = previewUrlsRef.current.get(id);
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      previewUrlsRef.current.delete(id);
    }
  };

  const reorder: DragEndEvent = (event) => {
    if (event.canceled) return;
    const { source } = event.operation;

    if (isSortable(source)) {
      const { initialIndex, index } = source;

      if (initialIndex !== index) {
        setImages((items) => move(items, event));
      }
    }
  };

  return { images, upload, remove, reorder };
}
