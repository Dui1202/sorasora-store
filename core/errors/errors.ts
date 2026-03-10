export type AppErrorCode =
  | "UNSUPPORTED_FILE_TYPE"
  | "FILE_TOO_LARGE"
  | "UPLOAD_FAILED"
  | "INVALID_DRAFT_ID"
  | "INVALID_PRODUCT_ID";

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
