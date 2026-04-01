import { UserRole } from "@/app/generated/prisma/enums";
import { AppError } from "@/core/errors/errors";
import { toErrorResponse } from "@/core/errors/http";
import { requireApiRole } from "@/lib/auth/authorization";
import { parseJsonBody } from "@/lib/http/parse-json-body";
import { parseMultipartFormData } from "@/lib/http/parse-multipart-form-data";
import {
  archiveProductService,
  getProductWithRelationsByIdService,
  updateProductService,
} from "@/lib/products";
import { updateProductPublishStateService } from "@/lib/products/services/update-product.service";
import { updateProductPublishStateSchema } from "@/lib/validation-schema/product-schema";
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
    const contentType = request.headers.get("content-type") ?? "";
    let updatedProduct;
    if (contentType.includes("application/json")) {
      updatedProduct = await handleJsonPatch(request, id);
    } else if (contentType.includes("multipart/form-data")) {
      updatedProduct = await handleMultipartPatch(request, id);
    } else {
      throw new AppError(
        "UNSUPPORTED_CONTENT_TYPE",
        "Unsupported content type. Use application/json or multipart/form-data.",
        415,
      );
    }

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

async function handleJsonPatch(request: NextRequest, id: string) {
  const jsonBody = await parseJsonBody(request);
  const validation = updateProductPublishStateSchema.safeParse(jsonBody);
  if (!validation.success) {
    throw new AppError("INVALID_REQUEST_BODY", "Invalid request body", 400);
  }
  const updatedProduct = await updateProductPublishStateService(
    id,
    validation.data.isPublished,
  );

  return updatedProduct;
}

async function handleMultipartPatch(request: NextRequest, id: string) {
  const formData = await parseMultipartFormData(request);
  return await updateProductService(id, formData);
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
