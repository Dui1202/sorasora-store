import { Prisma } from "@/app/generated/prisma/client";
import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/infrastructure/database/prisma";
import {
  removeProductImages,
  uploadProductImages,
} from "@/lib/infrastructure/storage/server";
import { validateProductFormData } from "@/lib/products";
import {
  CreateProductCommandData,
  ProductImages,
  ProductVariantImages,
  toCreateProductCommand,
} from "@/lib/validation-schema/product-schema";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

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

  const formValidation = validateProductFormData(formData);

  if (!formValidation.success) {
    return NextResponse.json({ error: formValidation.error }, { status: 400 });
  }
  // Process the creation command with validated data and uploaded files
  const uploadedProductImages: ProductImages = [];
  const uploadedVariantImages: ProductVariantImages = [];

  try {
    for (const image of formValidation.data.productImages) {
      const bytes = await image.file.arrayBuffer();
      const uploadResult = await uploadProductImages(Buffer.from(bytes));

      uploadedProductImages.push({
        path: uploadResult.path,
        alt: image.alt,
        position: image.position,
      });
    }

    if (formValidation.data.hasVariation) {
      for (const variant of formValidation.data.variants) {
        if (!variant.image) {
          throw new Error("Variant image is required for every variant.");
        }

        const bytes = await variant.image.file.arrayBuffer();
        const uploadResult = await uploadProductImages(Buffer.from(bytes));

        uploadedVariantImages.push({
          path: uploadResult.path,
          alt: variant.image.alt,
        });
      }
    }

    const command = toCreateProductCommand(
      formValidation.data,
      uploadedProductImages,
      uploadedVariantImages,
    );
    const newProduct = await createProduct(command);

    return NextResponse.json(newProduct, { status: 201 });
  } catch (error) {
    if (uploadedProductImages.length > 0) {
      try {
        await removeProductImages([
          ...uploadedProductImages.map((img) => img.path),
          ...uploadedVariantImages.map((img) => img.path),
        ]);
      } catch (cleanupError) {
        console.error(
          "Failed to clean up uploaded images after create failure",
          cleanupError,
        );
      }
    }

    if (error instanceof AppError) {
      return NextResponse.json(
        {
          code: error.code,
          message: error.message,
          detail: error.detail,
        },
        { status: error.status },
      );
    }

    if (error instanceof Prisma.PrismaClientKnownRequestError) {
      if (error.code === "P2002") {
        return NextResponse.json(
          { error: "SKU already exists" },
          { status: 409 },
        );
      }
    }

    if (error instanceof z.ZodError) {
      return NextResponse.json(
        {
          error: "Validation failed",
          issues: z.treeifyError(error),
        },
        { status: 400 },
      );
    }

    return NextResponse.json(
      { error: "Failed to create product" },
      { status: 500 },
    );
  }
}

async function createProduct(command: CreateProductCommandData) {
  const {
    name,
    description,
    variants,
    supplierCountry,
    franchiseId,
    productImages,
    isPublished,
  } = command;

  return prisma.product.create({
    data: {
      name,
      description: description || undefined,
      supplierCountry,
      franchiseId,
      isPublished: isPublished,
      variants: {
        create: variants.map((variant) => ({
          name: variant.name,
          sku: variant.sku,
          costPrice: variant.costPrice,
          sellingPrice: variant.sellingPrice,
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
}
