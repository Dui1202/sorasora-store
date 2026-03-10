import { AppError } from "@/core/errors/errors";
import { cleanUpTempProductImages } from "@/lib/storage/cleanUpTempProductImages";
import { NextRequest, NextResponse } from "next/server";

export async function DELETE(
  _request: NextRequest,
  { params }: { params: Promise<{ draftsId: string }> },
) {
  const { draftsId } = await params;

  try {
    await cleanUpTempProductImages(draftsId);
    return new NextResponse(null, { status: 204 });
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

    console.error("Failed to clean up temp product variant images", {
      draftsId,
      error,
    });

    return NextResponse.json(
      {
        code: "INTERNAL_SERVER_ERROR",
        message: "Failed to clean up temp product variant images.",
      },
      { status: 500 },
    );
  }
}
