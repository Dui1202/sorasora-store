export type AppErrorCode =
  | "INVALID_MULTIPART_FORM_DATA"
  | "UNSUPPORTED_FILE_TYPE"
  | "FILE_TOO_LARGE"
  | "UPLOAD_FAILED"
  | "INVALID_PRODUCT_ID"
  | "REMOVE_IMAGE_FAILED"
  | "CLEANUP_PRODUCT_IMAGES_FAILED";

export class AppError extends Error {
  constructor(
    public code: AppErrorCode,
    message: string,
    public status: number,
    public detail?: Record<string, unknown>,
  ) {
    super(message);
    this.name = "AppError";
  }
}
