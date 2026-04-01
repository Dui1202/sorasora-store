import { AppError } from "@/core/errors/errors";
import { NextRequest } from "next/server";

export const parseJsonBody = async (request: NextRequest): Promise<unknown> => {
  const contentType = request.headers.get("content-type") ?? "";

  if (!contentType.includes("application/json")) {
    throw new AppError(
      "UNSUPPORTED_CONTENT_TYPE",
      "Unsupported content type. Use application/json.",
      415,
    );
  }

  let jsonBody: unknown;

  try {
    jsonBody = await request.json();
  } catch {
    throw new AppError("INVALID_JSON_BODY", "Invalid JSON body", 400);
  }

  return jsonBody;
};
