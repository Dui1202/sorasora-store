import { cleanUpTempProductImage } from "@/lib/storage/cleanUpTempProductImage";
import { NextRequest, NextResponse } from "next/server";

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ draftsId: string; imageId: string }> },
) {
  const { draftsId, imageId } = await params;
  try {
    await cleanUpTempProductImage(imageId, draftsId);
  } catch (error) {
    return NextResponse.json(
      {
        code: "INTERNAL_SERVER_ERROR",
        message: "Failed to delete temp product image.",
        detail: error instanceof Error ? error.message : null,
      },
      { status: 500 },
    );
  }

  return new NextResponse(null, { status: 204 });
}
