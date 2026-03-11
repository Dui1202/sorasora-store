import { removeProductImages } from "@/lib/storage/removeProductImages";
import { NextRequest, NextResponse } from "next/server";
import { z } from "zod";

const PRODUCT_IMAGE_PATH_PREFIX = "products/product-images/";

const cleanUpProductImagesSchema = z.object({
  paths: z
    .array(z.string().trim().min(1))
    .min(1, "At least one path is required")
    .max(100, "You can only clean up up to 100 images per request"),
});

export async function POST(request: NextRequest) {
  let body: unknown;

  try {
    body = await request.json();
  } catch {
    return NextResponse.json(
      { code: "INVALID_JSON_BODY", message: "Invalid JSON body." },
      { status: 400 },
    );
  }

  const validation = cleanUpProductImagesSchema.safeParse(body);

  if (!validation.success) {
    return NextResponse.json(
      {
        code: "INVALID_CLEANUP_PAYLOAD",
        message: "Invalid cleanup payload.",
        issues: z.treeifyError(validation.error),
      },
      { status: 400 },
    );
  }

  const paths = Array.from(new Set(validation.data.paths));
  const hasInvalidPath = paths.some(
    (path) =>
      !path.startsWith(PRODUCT_IMAGE_PATH_PREFIX) || path.includes(".."),
  );

  if (hasInvalidPath) {
    return NextResponse.json(
      {
        code: "INVALID_IMAGE_PATH",
        message: "One or more image paths are invalid.",
      },
      { status: 400 },
    );
  }

  try {
    await removeProductImages(paths);
  } catch (error) {
    return NextResponse.json(
      {
        code: "CLEANUP_FAILED",
        message: "Failed to clean up uploaded product images.",
        detail: error instanceof Error ? error.message : null,
      },
      { status: 500 },
    );
  }

  return NextResponse.json({ removed: paths.length }, { status: 200 });
}
