import { ImagePlus } from "lucide-react";
import Image from "next/image";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { VariantDraftRow } from "./use-variant-combinations";
import {
  getImageGroupKey,
  toImageInputId,
} from "./variation-price-table-field-utils";

type VariantImageInputProps = {
  row: VariantDraftRow;
  onImageSelected: (groupKey: string, file: File) => void;
};

const VariantImageInput = ({
  row,
  onImageSelected,
}: VariantImageInputProps) => {
  const groupKey = getImageGroupKey(row);
  const inputId = toImageInputId(groupKey);
  const previewSrc = row.imagePreviewUrl
    ? row.imagePreviewUrl
    : row.imagePath
      ? getProductImagePublicUrl(row.imagePath)
      : null;
  const hasImage = Boolean(row.imageFile || row.imagePath);

  return (
    <div className="space-y-2">
      <input
        id={inputId}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        hidden
        onChange={(event) => {
          const selectedFile = event.target.files?.[0];

          if (!selectedFile) {
            return;
          }

          onImageSelected(groupKey, selectedFile);
          event.target.value = "";
        }}
      />

      <label
        htmlFor={inputId}
        className="relative flex h-20 w-20 cursor-pointer items-center justify-center overflow-hidden rounded-md border border-dashed"
      >
        {previewSrc ? (
          <Image
            src={previewSrc}
            alt={row.imageAlt || "Variant image preview"}
            fill
            sizes="80px"
            unoptimized
            className="object-cover"
          />
        ) : (
          <ImagePlus className="text-muted-foreground" />
        )}
      </label>

      {!hasImage && (
        <p className="text-xs text-destructive">Variant image is required</p>
      )}
    </div>
  );
};

export default VariantImageInput;
