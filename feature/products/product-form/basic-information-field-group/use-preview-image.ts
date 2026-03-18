import { useEffect, useRef, useState } from "react";
import type { UploadedImagePreview } from "./type";
import { DragEndEvent } from "@dnd-kit/react";
import { isSortable } from "@dnd-kit/react/sortable";
import { move } from "@dnd-kit/helpers";

export function usePreviewImage() {
  const [images, setImages] = useState<UploadedImagePreview[]>([]);
  const previewUrlsRef = useRef(new Map<string, string>());

  useEffect(() => {
    const previewUrls = previewUrlsRef.current;

    return () => {
      for (const previewUrl of previewUrls.values()) {
        URL.revokeObjectURL(previewUrl);
      }

      previewUrls.clear();
    };
  }, []);

  const addImages = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(event.target.files || []);
    if (files.length === 0) {
      return;
    }

    setImages((prev) => {
      const createdPreviews: UploadedImagePreview[] = files.map(
        (file, index) => {
          const id = crypto.randomUUID();
          const previewUrl = URL.createObjectURL(file);
          previewUrlsRef.current.set(id, previewUrl);

          return {
            id,
            file,
            fileName: file.name,
            previewUrl,
            position: prev.length + index,
          };
        },
      );

      return [...prev, ...createdPreviews];
    });

    event.target.value = "";
  };

  const remove = (id: string) => {
    setImages((prev) => {
      return prev
        .filter((img) => img.id !== id)
        .map((img, index) => ({
          ...img,
          position: index,
        }));
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
        setImages((items) =>
          move(items, event).map((item, itemIndex) => ({
            ...item,
            position: itemIndex,
          })),
        );
      }
    }
  };

  return { images, addImages, remove, reorder };
}
