import { UserRole } from "@/app/generated/prisma/enums";
import { AppError } from "@/core/errors/errors";
import { toErrorResponse } from "@/core/errors/http";
import { requireApiRole } from "@/lib/auth/authorization";
import { parseMultipartFormData } from "@/lib/http/parse-multipart-form-data";
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

    const formData = await parseMultipartFormData(request);

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
      throw new AppError(
        "PRODUCT_NOT_FOUND",
        `Product with ID ${id} not found`,
        404,
      );
    }

    return NextResponse.json(product, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
