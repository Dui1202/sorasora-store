import { AppError } from "@/core/errors/errors";
import { supabase } from "../supabase";
import { fileTypeFromBuffer, FileTypeResult } from "file-type";

const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
const allowedImageContentTypes = ["image/jpeg", "image/png", "image/webp"];

type ValidateFileType =
  | { isValid: true; fileType: FileTypeResult }
  | { isValid: false };

export async function uploadTempProductVariantImage(file: Buffer) {
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

  const filePath = `products/temp/${crypto.randomUUID()}.${ext}`;

  const { error } = await supabase.storage
    .from("product-images")
    .upload(filePath, file, {
      contentType,
    });

  if (error) {
    throw new AppError("UPLOAD_FAILED", "Failed to upload image.", 500, {
      cause: error.message,
    });
  }

  const { data: publicUrlData } = supabase.storage
    .from("product-images")
    .getPublicUrl(filePath);

  return {
    path: filePath,
    publicUrl: publicUrlData.publicUrl,
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
  if (file.length > MAX_FILE_SIZE) {
    return false;
  }

  return true;
}
