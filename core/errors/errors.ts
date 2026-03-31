import { AppErrorCode } from "./type";

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
