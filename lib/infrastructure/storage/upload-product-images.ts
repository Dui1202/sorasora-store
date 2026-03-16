import { AppError } from "@/core/errors/errors";
import { supabase } from "./supabase";
import { fileTypeFromBuffer, FileTypeResult } from "file-type";

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const allowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];

type ValidateFileType =
  | { isValid: true; fileType: FileTypeResult }
  | { isValid: false };

export async function uploadProductImages(file: Buffer) {
  if (!validateFileSize(file)) {
    throw new AppError(
      "FILE_TOO_LARGE",
      "The file size must not be greater than 5MB.",
      400,
    );
  }

  const result = await validateFileType(file);

  if (!result.isValid) {
    throw new AppError(
      "UNSUPPORTED_FILE_TYPE",
      "Only JPEG, PNG, and WebP images are allowed.",
      400,
    );
  }

  const { mime: contentType, ext } = result.fileType;
  const filePath = `products/product-images/${crypto.randomUUID()}.${ext}`;

  const { error } = await supabase.storage
    .from("product-images")
    .upload(filePath, file, {
      contentType,
      upsert: false,
    });

  if (error) {
    throw new AppError("UPLOAD_FAILED", "Failed to upload image.", 500, {
      cause: error.message,
    });
  }

  return {
    path: filePath,
  };
}

async function validateFileType(file: Buffer): Promise<ValidateFileType> {
  const fileType = await fileTypeFromBuffer(file);

  if (!fileType) {
    return { isValid: false };
  }

  if (!allowedImageContentTypes.includes(fileType.mime)) {
    return { isValid: false };
  }

  return { isValid: true, fileType };
}

function validateFileSize(file: Buffer): boolean {
  return file.length <= MAX_FILE_SIZE;
}
