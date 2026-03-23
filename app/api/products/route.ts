import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import { createProductService } from "@/lib/products";
import { NextRequest, NextResponse } from "next/server";

export async function GET() {
  const products = await prisma.product.findMany({
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!products) {
    return NextResponse.json(
      { error: "Failed to fetch products" },
      { status: 500 },
    );
  }

  return NextResponse.json(products);
}

export async function POST(request: NextRequest) {
  const contentType = request.headers.get("content-type") ?? "";

  if (contentType.includes("multipart/form-data")) {
    return handleMultipartCreate(request);
  }

  return NextResponse.json(
    { error: "Unsupported content type. Use multipart/form-data." },
    { status: 415 },
  );
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
