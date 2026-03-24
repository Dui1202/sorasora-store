import { AppError } from "@/core/errors/errors";

export function createMultipartValidationError(
  message: string,
  detail?: Record<string, unknown>,
) {
  return new AppError("INVALID_MULTIPART_FORM_DATA", message, 400, detail);
}
