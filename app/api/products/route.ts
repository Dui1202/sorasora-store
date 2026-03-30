import { UserRole } from "@/app/generated/prisma/enums";
import { toErrorResponse } from "@/core/errors/http";
import { requireApiRole } from "@/lib/auth/authorization";
import { createProductService, listProductsService } from "@/lib/products";
import { NextRequest, NextResponse } from "next/server";

const ADMIN_ROLES = [UserRole.STAFF, UserRole.ADMIN];

export async function GET(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);
    const products = await listProductsService();
    return NextResponse.json(products);
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function POST(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

    const contentType = request.headers.get("content-type") ?? "";

    if (contentType.includes("multipart/form-data")) {
      return handleMultipartCreate(request);
    }

    return NextResponse.json(
      { error: "Unsupported content type. Use multipart/form-data." },
      { status: 415 },
    );
  } catch (error) {
    return toErrorResponse(error);
  }
}

async function handleMultipartCreate(request: NextRequest) {
  let formData: FormData;

  //  Default error
  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data" },
      { status: 400 },
    );
  }

  try {
    const newProduct = await createProductService(formData);

    return NextResponse.json(newProduct, { status: 201 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
