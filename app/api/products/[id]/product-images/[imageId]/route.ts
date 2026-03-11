import prisma from "@/lib/prisma";
import { removeProductImage } from "@/lib/storage/removeProductImage";
import { NextRequest, NextResponse } from "next/server";

export async function DELETE(
  _request: NextRequest,
  { params }: { params: Promise<{ id: string; imageId: string }> },
) {
  const { id: productId, imageId } = await params;

  const productImage = await prisma.productImage.findFirst({
    where: {
      id: imageId,
      productId,
    },
    select: {
      id: true,
      path: true,
    },
  });

  if (!productImage) {
    return NextResponse.json(
      {
        code: "IMAGE_NOT_FOUND",
        message: "Product image not found.",
      },
      { status: 404 },
    );
  }

  try {
    await removeProductImage(productImage.path);
    await prisma.productImage.delete({
      where: {
        id: productImage.id,
      },
    });
  } catch (error) {
    return NextResponse.json(
      {
        code: "INTERNAL_SERVER_ERROR",
        message: "Failed to delete product image.",
        detail: error instanceof Error ? error.message : null,
      },
      { status: 500 },
    );
  }

  return new NextResponse(null, { status: 204 });
}
