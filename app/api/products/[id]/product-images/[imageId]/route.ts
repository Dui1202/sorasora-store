import { removeProductImage } from "@/lib/storage/removeProductImage";
import { NextRequest, NextResponse } from "next/server";

export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string; imageId: string }> },
) {
  const { imageId } = await params;
  try {
    await removeProductImage(imageId);
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
