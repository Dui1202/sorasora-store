export type AppErrorCode =
  | "INVALID_MULTIPART_FORM_DATA"
  | "UNSUPPORTED_FILE_TYPE"
  | "FILE_TOO_LARGE"
  | "UPLOAD_FAILED"
  | "WAREHOUSE_NOT_FOUND"
  | "INVALID_PRODUCT_ID"
  | "INVALID_PRODUCT_STOCK_STATE"
  | "REMOVE_IMAGE_FAILED"
  | "PRODUCT_NOT_FOUND"
  | "CLEANUP_PRODUCT_IMAGES_FAILED"
  | "ARCHIVE_PRODUCT_IMAGE_CLEANUP_FAILED"
  | "UPDATE_PRODUCT_FAILED";

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
