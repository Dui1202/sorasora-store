import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import { removeProductImages } from "@/lib/infrastructure/storage/server";
import { getProductInventoryState } from "@/lib/products/inventory/product-inventory";
import { updateProductService } from "@/lib/products/services/update-product.service";
import { NextRequest, NextResponse } from "next/server";

type Params = {
  params: Promise<{ id: string }>;
};

export async function DELETE(_request: NextRequest, { params }: Params) {
  try {
    const { id } = await params;
    const product = await prisma.product.findUnique({
      where: {
        id,
      },
      include: {
        productImages: true,
        variants: {
          include: {
            image: true,
            stocks: {
              include: {
                warehouse: true,
              },
            },
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

    const deleteProduct = await prisma.$transaction(async (tx) => {
      await tx.stock.deleteMany({
        where: {
          variant: {
            productId: id,
          },
        },
      });

      return tx.product.delete({
        where: {
          id,
        },
      });
    });

    return NextResponse.json(deleteProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function PATCH(request: NextRequest, { params }: Params) {
  const { id } = await params;

  let formData: FormData;

  try {
    formData = await request.formData();
  } catch {
    return NextResponse.json(
      { error: "Invalid multipart form data" },
      { status: 400 },
    );
  }

  try {
    const updatedProduct = await updateProductService(id, formData);

    return NextResponse.json(updatedProduct, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function GET(_request: NextRequest, { params }: Params) {
  try {
    const { id } = await params;

    const product = await prisma.product.findUnique({
      where: {
        id,
      },
      include: {
        productImages: true,
        variants: {
          include: {
            image: true,
            stocks: {
              include: {
                warehouse: true,
              },
            },
          },
        },
      },
    });

    if (!product) {
      return NextResponse.json({ error: "Product not found" }, { status: 404 });
    }

    getProductInventoryState(product);

    return NextResponse.json(product, { status: 200 });
  } catch (error) {
    return toErrorResponse(error);
  }
}
