import { Prisma } from "@/app/generated/prisma/client";
import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/prisma";
import { removeProductImages } from "@/lib/storage/removeProductImages";
import { uploadProductImages } from "@/lib/storage/uploadProductImages";
import {
  CreateProductCommandData,
  ProductImages,
  createProductFormSchema,
  toCreateProductCommand,
} from "@/lib/validation-schema/product-schema/productSchema";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

const multipartPayloadSchema = createProductFormSchema
  .omit({ productImages: true })
  .extend({
    productImages: z
      .array(
        z.object({
          alt: z.string().trim().optional(),
          position: z.coerce.number().int().min(0).default(0),
        }),
      )
      .min(1, "Upload at least 1 product image!"),
  });

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

  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data" },
      { status: 400 },
    );
  }

  const payloadField = formData.get("payload");

  if (typeof payloadField !== "string") {
    return NextResponse.json(
      { error: "Missing payload field" },
      { status: 400 },
    );
  }

  let parsedPayload: unknown;

  try {
    parsedPayload = JSON.parse(payloadField);
  } catch {
    return NextResponse.json(
      { error: "Invalid payload JSON" },
      { status: 400 },
    );
  }

  const payloadValidation = multipartPayloadSchema.safeParse(parsedPayload);

  if (!payloadValidation.success) {
    return NextResponse.json(
      {
        error: "Validation failed",
        issues: z.treeifyError(payloadValidation.error),
      },
      { status: 400 },
    );
  }

  const imageFiles = formData
    .getAll("images")
    .filter((value): value is File => value instanceof File);

  if (imageFiles.length !== payloadValidation.data.productImages.length) {
    return NextResponse.json(
      { error: "Image files count does not match product image metadata." },
      { status: 400 },
    );
  }

  const formDataCandidate = {
    ...payloadValidation.data,
    productImages: payloadValidation.data.productImages.map((image, index) => ({
      file: imageFiles[index],
      alt: image.alt,
      position: image.position,
    })),
  };

  const formValidation = createProductFormSchema.safeParse(formDataCandidate);

  if (!formValidation.success) {
    return NextResponse.json(
      {
        error: "Validation failed",
        issues: z.treeifyError(formValidation.error),
      },
      { status: 400 },
    );
  }

  const uploadedProductImages: ProductImages = [];

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

    const command = toCreateProductCommand(
      formValidation.data,
      uploadedProductImages,
    );
    const newProduct = await createProduct(command);

    return NextResponse.json(newProduct, { status: 201 });
  } catch (error) {
    if (uploadedProductImages.length > 0) {
      try {
        await removeProductImages(uploadedProductImages.map((img) => img.path));
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
  } = command;

  return prisma.product.create({
    data: {
      name,
      description: description || undefined,
      supplierCountry,
      franchiseId,
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

