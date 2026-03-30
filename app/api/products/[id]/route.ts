import { toErrorResponse } from "@/core/errors/http";
import {
  archiveProductService,
  getProductInventoryState,
  getProductByIdService,
  updateProductService,
} from "@/lib/products";
import { NextRequest, NextResponse } from "next/server";

type Params = {
  params: Promise<{ id: string }>;
};

export async function DELETE(_request: NextRequest, { params }: Params) {
  try {
    const { id } = await params;
    const archivedProduct = await archiveProductService(id);
    return NextResponse.json(archivedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function PATCH(request: NextRequest, { params }: Params) {
  const { id } = await params;

  let formData: FormData;

  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data" },
      { status: 400 },
    );
  }

  try {
    const updatedProduct = await updateProductService(id, formData);

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function GET(_request: NextRequest, { params }: Params) {
  try {
    const { id } = await params;

    const product = await getProductByIdService(id, true);

    if (!product) {
      return NextResponse.json({ error: "Product not found" }, { status: 404 });
    }

    getProductInventoryState(product);

    return NextResponse.json(product, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
