import { Prisma } from "@/app/generated/prisma/browser";
import prisma from "@/lib/infrastructure/database/prisma";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { validateProductFormData } from "@/lib/products";
import { validateUpdateProductFormData } from "@/lib/products/product-validation";
import { NextRequest, NextResponse } from "next/server";
import { unknown } from "zod";

type Params = {
  params: Promise<{ id: string }>;
};

export async function DELETE(_request: NextRequest, { params }: Params) {
  const { id } = await params;
  const product = await prisma.product.findUnique({
    where: {
      id: id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  await removeProductImages(product.productImages.map((image) => image.path));

  if (product.variants.length > 1 && product.variants[0].image) {
    await removeProductImages(
      product.variants.map((variant) => variant.image!.path),
    );
  }

  const deleteProduct = await prisma.product.delete({
    where: {
      id: id,
    },
  });

  return NextResponse.json(deleteProduct, { status: 200 });
}

export async function PATCH(request: NextRequest, { params }: Params) {
  const { id } = await params;

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

  const product = await prisma.product.findUnique({
    where: {
      id: id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  const formValidation = validateUpdateProductFormData(formData);

  if (!formValidation.success) {
    return NextResponse.json({ error: formValidation.error }, { status: 400 });
  }
}

export async function GET(_request: NextRequest, { params }: Params) {
  const { id } = await params;

  const product = await prisma.product.findUnique({
    where: {
      id: id,
    },
    include: {
      productImages: true,
      variants: {
        include: {
          image: true,
        },
      },
    },
  });

  if (!product) {
    return NextResponse.json({ error: "Product not found" }, { status: 404 });
  }

  return NextResponse.json(product, { status: 200 });
}
