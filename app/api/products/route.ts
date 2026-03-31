import { UserRole } from "@/app/generated/prisma/enums";
import { toErrorResponse } from "@/core/errors/http";
import { requireApiRole } from "@/lib/auth/authorization";
import { parseMultipartFormData } from "@/lib/http/parse-multipart-form-data";
import { createProductService, listProductsService } from "@/lib/products";
import { NextRequest, NextResponse } from "next/server";

const ADMIN_ROLES = [UserRole.STAFF, UserRole.ADMIN];

export async function GET(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

    const products = await listProductsService();

    return NextResponse.json(products, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function POST(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

    const formData = await parseMultipartFormData(request);

    const newProduct = await createProductService(formData);

    return NextResponse.json(newProduct, { status: 201 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
