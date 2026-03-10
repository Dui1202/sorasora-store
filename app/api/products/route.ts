import prisma from "@/lib/prisma";
import { createProductCommandSchema } from "@/lib/validationSchema";
import { Prisma } from "@/app/generated/prisma/client";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

export async function POST(request: NextRequest) {
  let body: unknown;

  try {
    body = await request.json();
  } catch {
    return NextResponse.json({ error: "Invalid JSON body" }, { status: 400 });
  }

  const validation = createProductCommandSchema.safeParse(body);

  if (!validation.success) {
    return NextResponse.json(
      {
        error: "Validation failed",
        issues: z.treeifyError(validation.error),
      },
      { status: 400 },
    );
  }

  const {
    name,
    description,
    variants,
    supplierCountry,
    franchiseId,
    productImages,
  } = validation.data;

  try {
    const newProduct = await prisma.product.create({
      data: {
        name,
        description: description || undefined,
        supplierCountry,
        franchiseId,
        variants: {
          create: variants.map((variant) => ({
            name: variant.name,
            sku: variant.sku,
            originalPrice: variant.originalPrice,
            salePrice: variant.salePrice,
            position: variant.position,
            image: variant.image
              ? {
                  create: {
                    url: variant.image.url,
                    alt: variant.image.alt,
                  },
                }
              : undefined,
          })),
        },
        productImages: {
          create: productImages.map((productImage) => ({
            url: productImage.url,
            alt: productImage.alt,
            position: productImage.position,
          })),
        },
      },
      include: {
        variants: {
          include: {
            image: true,
          },
        },
        productImages: true,
      },
    });

    return NextResponse.json(newProduct, { status: 201 });
  } catch (error) {
    if (error instanceof Prisma.PrismaClientKnownRequestError) {
      if (error.code === "P2002") {
        return NextResponse.json(
          { error: "SKU already exists" },
          { status: 409 },
        );
      }
    }

    return NextResponse.json(
      { error: "Failed to create product" },
      { status: 500 },
    );
  }
}
