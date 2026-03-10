import { AppError } from "@/core/errors/errors";
import { uploadProductImages } from "@/lib/storage/uploadProductImages";
import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  let formData: FormData;

  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { code: "INVALID_FORM_DATA", message: "Invalid multipart form data." },
      { status: 400 },
    );
  }

  const image = formData.get("image") ?? formData.get("file");

  if (!(image instanceof File)) {
    return NextResponse.json(
      { code: "IMAGE_REQUIRED", message: "An image file is required." },
      { status: 400 },
    );
  }

  const bytes = await image.arrayBuffer();
  const buffer = Buffer.from(bytes);

  try {
    const uploadResult = await uploadProductImages(buffer);
    return NextResponse.json(uploadResult, { status: 201 });
  } catch (error) {
    if (error instanceof AppError) {
      return NextResponse.json(
        {
          code: error.code,
          message: error.message,
          detail: error.detail,
        },
        { status: error.status },
      );
    }

    return NextResponse.json(
      {
        code: "INTERNAL_SERVER_ERROR",
        message: "Failed to upload product image.",
      },
      { status: 500 },
    );
  }
}
