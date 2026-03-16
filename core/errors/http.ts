import { NextResponse } from "next/server";
import { AppError } from "./errors";
import z from "zod";
import { Prisma } from "@/app/generated/prisma/client";

export function toErrorResponse(error: unknown) {
  if (error instanceof AppError) {
    return NextResponse.json(
      { code: error.code, message: error.message, details: error.detail },
      { status: error.status },
    );
  }

  if (error instanceof z.ZodError) {
    return NextResponse.json(
      {
        code: "VALIDATION_ERROR",
        message: "Validation failed",
        details: z.treeifyError(error),
      },
      { status: 400 },
    );
  }

  if (
    error instanceof Prisma.PrismaClientKnownRequestError &&
    error.code === "P2002"
  ) {
    return NextResponse.json(
      { code: "DUPLICATE_RESOURCE", message: "Resource already exists" },
      { status: 409 },
    );
  }

  return NextResponse.json(
    { code: "INTERNAL_SERVER_ERROR", message: "Unexpected server error" },
    { status: 500 },
  );
}
