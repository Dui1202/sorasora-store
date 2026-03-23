import { SupplierCountry } from "@/app/generated/prisma/enums";
import type {
  CreateProductFormValidationData,
  UpdateProductFormValidationData,
} from "@/lib/validation-schema/product-schema";
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
      clientKey: "variant-strawberry",
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

const createUpdateValidationData = (
  overrides: Partial<UpdateProductFormValidationData> = {},
): UpdateProductFormValidationData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  isPublished: false,
  hasVariation: true,
  baseSku: undefined,
  baseCostPrice: undefined,
  baseSellingPrice: undefined,
  existProductImages: [
    {
      path: "products/existing-main.png",
      alt: "Existing main",
      position: 0,
    },
  ],
  newProductImages: [],
  variants: [
    {
      name: "Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      position: 0,
      existImage: {
        path: "products/variants/existing-strawberry.png",
        alt: "Existing strawberry",
      },
      newImage: undefined,
    },
    {
      name: "Chocolate",
      sku: "POCKY-CHOCOLATE",
      costPrice: 110,
      sellingPrice: 170,
      position: 1,
      existImage: undefined,
      newImage: {
        file: createFile("variant-2.png"),
        alt: "New chocolate",
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
      createValidationData().productImages,
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

  it("uploads variant images keyed by stable client ids", async () => {
    uploadProductImagesMock.mockResolvedValueOnce({
      path: "products/variants/strawberry.png",
    });

    const uploadedImages = await uploadProductVariantImagesService(
      createValidationData().variants,
    );

    expect(uploadedImages).toEqual(
      new Map([
        [
          "variant-strawberry",
          {
            path: "products/variants/strawberry.png",
            alt: "Variant front",
          },
        ],
      ]),
    );
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(1);
  });

  it("uploads only update variant newImages in variant order", async () => {
    uploadProductImagesMock.mockResolvedValueOnce({
      path: "products/variants/chocolate.png",
    });

    const uploadedImages = await uploadProductVariantImagesService(
      createUpdateValidationData().variants,
      {
        mode: "update",
      },
    );

    expect(uploadedImages).toEqual([
      {
        path: "products/variants/chocolate.png",
        alt: "New chocolate",
      },
    ]);
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(1);
  });

  it("throws when a variant image is missing for a variation product", async () => {
    const data = createValidationData({
      variants: [
        {
          clientKey: "variant-strawberry",
          name: "Strawberry",
          sku: "POCKY-STRAWBERRY",
          costPrice: 120,
          sellingPrice: 180,
          position: 0,
          image: undefined,
        },
      ],
    });

    await expect(
      uploadProductVariantImagesService(data.variants),
    ).rejects.toThrow("Variant image is required for every variant.");
    expect(uploadProductImagesMock).not.toHaveBeenCalled();
  });

  it("cleans up uploaded product and variant image paths together", async () => {
    await cleanUpProductImagesService(
      [{ path: "products/main.png", alt: "Front angle", position: 0 }],
      new Map([
        [
          "variant-strawberry",
          {
            path: "products/variants/strawberry.png",
            alt: "Variant front",
          },
        ],
      ]),
    );

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/main.png",
      "products/variants/strawberry.png",
    ]);
  });

  it("cleans up uploaded variant image paths even when no new product images were uploaded", async () => {
    await cleanUpProductImagesService([], [
      {
        path: "products/variants/chocolate.png",
        alt: "Variant front",
      },
    ]);

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/variants/chocolate.png",
    ]);
  });

  it("maps cleanup storage failures to an AppError", async () => {
    removeProductImagesMock.mockRejectedValueOnce(new Error("storage down"));

    await expect(
      cleanUpProductImagesService(
        [{ path: "products/main.png", alt: "Front angle", position: 0 }],
        new Map([
          [
            "variant-strawberry",
            {
              path: "products/variants/strawberry.png",
              alt: "Variant front",
            },
          ],
        ]),
      ),
    ).rejects.toMatchObject({
      code: "CLEANUP_PRODUCT_IMAGES_FAILED",
      status: 500,
    });
  });
});
