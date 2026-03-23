import { SupplierCountry } from "@/app/generated/prisma/enums";
import { AppError } from "@/core/errors/errors";
import type { CreateProductFormValidationData } from "@/lib/validation-schema/product-schema";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { removeProductImagesMock, uploadProductImagesMock } = vi.hoisted(() => ({
  uploadProductImagesMock: vi.fn(),
  removeProductImagesMock: vi.fn(),
}));

vi.mock("@/lib/infrastructure/storage/server", () => ({
  uploadProductImages: uploadProductImagesMock,
  removeProductImages: removeProductImagesMock,
}));

import {
  cleanUpProductImagesService,
  uploadProductImagesService,
  uploadProductVariantImagesService,
} from "./product-image.service";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createValidationData = (
  overrides: Partial<CreateProductFormValidationData> = {},
): CreateProductFormValidationData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  isPublished: false,
  hasVariation: true,
  baseSku: undefined,
  baseCostPrice: undefined,
  baseSellingPrice: undefined,
  productImages: [
    {
      file: createFile("product-1.png"),
      alt: "Front angle",
      position: 0,
    },
    {
      file: createFile("product-2.png"),
      alt: "Back angle",
      position: 1,
    },
  ],
  variants: [
    {
      name: "Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      position: 0,
      image: {
        file: createFile("variant-1.png"),
        alt: "Variant front",
      },
    },
  ],
  ...overrides,
});

describe("product-image.service", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("uploads product images and maps storage paths with metadata", async () => {
    uploadProductImagesMock
      .mockResolvedValueOnce({ path: "products/main.png" })
      .mockResolvedValueOnce({ path: "products/secondary.png" });

    const uploadedImages = await uploadProductImagesService(
      createValidationData(),
    );

    expect(uploadedImages).toEqual([
      {
        path: "products/main.png",
        alt: "Front angle",
        position: 0,
      },
      {
        path: "products/secondary.png",
        alt: "Back angle",
        position: 1,
      },
    ]);
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(2);
    expect(uploadProductImagesMock.mock.calls[0]?.[0]).toBeInstanceOf(Buffer);
  });

  it("uploads variant images only when variations are enabled", async () => {
    uploadProductImagesMock.mockResolvedValueOnce({
      path: "products/variants/strawberry.png",
    });

    const uploadedImages = await uploadProductVariantImagesService(
      createValidationData(),
    );

    expect(uploadedImages).toEqual([
      {
        path: "products/variants/strawberry.png",
        alt: "Variant front",
      },
    ]);
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(1);
  });

  it("throws when a variant image is missing for a variation product", async () => {
    const data = createValidationData({
      variants: [
        {
          name: "Strawberry",
          sku: "POCKY-STRAWBERRY",
          costPrice: 120,
          sellingPrice: 180,
          position: 0,
          image: undefined,
        },
      ],
    });

    await expect(uploadProductVariantImagesService(data)).rejects.toThrow(
      "Variant image is required for every variant.",
    );
    expect(uploadProductImagesMock).not.toHaveBeenCalled();
  });

  it("cleans up uploaded product and variant image paths together", async () => {
    await cleanUpProductImagesService(
      [{ path: "products/main.png", alt: "Front angle", position: 0 }],
      [{ path: "products/variants/strawberry.png", alt: "Variant front" }],
    );

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/main.png",
      "products/variants/strawberry.png",
    ]);
  });

  it("maps cleanup storage failures to an AppError", async () => {
    removeProductImagesMock.mockRejectedValueOnce(new Error("storage down"));

    await expect(
      cleanUpProductImagesService(
        [{ path: "products/main.png", alt: "Front angle", position: 0 }],
        [{ path: "products/variants/strawberry.png", alt: "Variant front" }],
      ),
    ).rejects.toMatchObject<AppError>({
      code: "CLEANUP_PRODUCT_IMAGES_FAILED",
      status: 500,
    });
  });
});
