import { AppError } from "@/core/errors/errors";
import { NextRequest } from "next/server";

export const parseMultipartFormData = async (
  request: NextRequest,
): Promise<FormData> => {
  const contentType = request.headers.get("content-type") ?? "";

  if (!contentType.includes("multipart/form-data")) {
    throw new AppError(
      "UNSUPPORTED_CONTENT_TYPE",
      "Unsupported content type. Use multipart/form-data.",
      415,
    );
  }

  let formData: FormData;

  try {
    formData = await request.formData();
  } catch {
    throw new AppError(
      "INVALID_MULTIPART_FORM_DATA",
      "Invalid multipart form data",
      400,
    );
  }

  return formData;
};
