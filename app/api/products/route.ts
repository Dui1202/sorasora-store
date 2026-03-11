import { Prisma } from "@/app/generated/prisma/client";
import prisma from "@/lib/prisma";
import {
  createProductCommandSchema,
  ProductImages,
} from "@/lib/validation-schema/product-schema/validationSchema";
import axios from "axios";
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
            image: {
              create: {
                path: variant.image.path,
                alt: variant.image.alt,
              },
            },
          })),
        },
        productImages: {
          create: productImages.map((productImage) => ({
            path: productImage.path,
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
    cleanUpProductImages(productImages);

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

const cleanUpProductImages = async (productImages: ProductImages) => {
  if (productImages.length === 0) {
    return;
  }

  try {
    await axios.post("/api/uploads/product-images/cleanup", {
      paths: productImages.map((image) => image.path),
    });
  } catch (error) {
    console.log("Failed to clean up product images:", error);
  }
};
