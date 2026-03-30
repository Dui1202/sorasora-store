import { UserRole } from "@/app/generated/prisma/enums";
import { toErrorResponse } from "@/core/errors/http";
import { requireApiRole } from "@/lib/auth/authorization";
import {
  archiveProductService,
  getProductWithRelationsByIdService,
  updateProductService,
} from "@/lib/products";
import { NextRequest, NextResponse } from "next/server";

type Params = {
  params: Promise<{ id: string }>;
};

const ADMIN_ROLES = [UserRole.STAFF, UserRole.ADMIN];

export async function DELETE(request: NextRequest, { params }: Params) {
  try {
    await requireApiRole(request, ADMIN_ROLES);
    const { id } = await params;
    const archivedProduct = await archiveProductService(id);
    return NextResponse.json(archivedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function PATCH(request: NextRequest, { params }: Params) {
  try {
    await requireApiRole(request, ADMIN_ROLES);
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

    const updatedProduct = await updateProductService(id, formData);

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function GET(request: NextRequest, { params }: Params) {
  try {
    await requireApiRole(request, ADMIN_ROLES);
    const { id } = await params;

    const product = await getProductWithRelationsByIdService(id);

    if (!product) {
      return NextResponse.json({ error: "Product not found" }, { status: 404 });
    }

    return NextResponse.json(product, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
