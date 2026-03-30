import { AppError } from "@/core/errors/errors";
import { SupplierCountry } from "@/app/generated/prisma/enums";
import { beforeEach, describe, expect, it, vi } from "vitest";

const {
  updateProductMock,
  findProductByIdMock,
  removeProductImagesMock,
  uploadProductImagesMock,
} = vi.hoisted(() => ({
  updateProductMock: vi.fn(),
  findProductByIdMock: vi.fn(),
  uploadProductImagesMock: vi.fn(),
  removeProductImagesMock: vi.fn(),
}));

vi.mock("@/lib/products/repositories/product.repository", () => ({
  updateProduct: updateProductMock,
  findProductById: findProductByIdMock,
}));

vi.mock("@/lib/infrastructure/storage/server", () => ({
  uploadProductImages: uploadProductImagesMock,
  removeProductImages: removeProductImagesMock,
}));

import { updateProductService } from "./update-product.service";

const EXISTING_VARIANT_ID = "ck1234567890abcde1234567";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createBasePayload = () => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
  hasVariation: true,
  baseVariantId: undefined,
  baseSku: undefined,
  baseCostPrice: undefined,
  baseSellingPrice: undefined,
  baseStock: undefined,
  existProductImages: [
    {
      path: "products/existing-main.png",
      alt: "Existing main",
      position: 0,
    },
  ],
  newProductImages: [
    {
      alt: "New detail",
      position: 1,
    },
  ],
  variants: [
    {
      id: EXISTING_VARIANT_ID,
      name: "Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      stock: {
        quantity: 5,
      },
      position: 0,
      existImage: {
        path: "products/variants/existing-strawberry.png",
        alt: "Existing strawberry",
      },
      newImage: undefined,
    },
    {
      id: undefined,
      name: "Chocolate",
      sku: "POCKY-CHOCOLATE",
      costPrice: 110,
      sellingPrice: 170,
      stock: {
        quantity: 7,
      },
      position: 1,
      existImage: undefined,
      newImage: {
        alt: "New chocolate",
      },
    },
  ],
});

const createMultipartFormData = ({
  payload = createBasePayload(),
  productFiles = [createFile("product-1.png")],
  variantFiles = [createFile("variant-1.png")],
}: {
  payload?: Record<string, unknown>;
  productFiles?: File[];
  variantFiles?: File[];
}) => {
  const formData = new FormData();

  formData.set("payload", JSON.stringify(payload));

  for (const file of productFiles) {
    formData.append("images", file);
  }

  for (const file of variantFiles) {
    formData.append("variantImages", file);
  }

  return formData;
};

describe("updateProductService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    findProductByIdMock.mockResolvedValue({
      id: "product_123",
      hasVariation: true,
      variants: [
        {
          id: EXISTING_VARIANT_ID,
          stocks: [
            {
              id: "stock_1",
              variantId: EXISTING_VARIANT_ID,
              warehouseId: "ckwarehouse1234567890abcde1",
              quantity: 4,
              warehouse: {
                id: "ckwarehouse1234567890abcde1",
                name: "Main Warehouse",
              },
            },
          ],
          image: {
            path: "products/variants/existing-strawberry.png",
            alt: "Existing strawberry",
          },
        },
      ],
      productImages: [
        {
          path: "products/existing-main.png",
          alt: "Existing main",
          position: 0,
        },
      ],
    });
  });

  it("uploads new product and variant images before updating the product", async () => {
    uploadProductImagesMock
      .mockResolvedValueOnce({
        path: "products/new-detail.png",
      })
      .mockResolvedValueOnce({
        path: "products/variants/new-chocolate.png",
      });
    updateProductMock.mockResolvedValueOnce({
      id: "product_123",
      name: "Pocky",
    });

    const result = await updateProductService(
      "product_123",
      createMultipartFormData({}),
    );

    expect(result).toEqual({
      id: "product_123",
      name: "Pocky",
    });
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(2);
    expect(updateProductMock).toHaveBeenCalledWith(
      "product_123",
      {
        name: "Pocky",
        description: "Chocolate biscuit sticks",
        supplierCountry: SupplierCountry.JAPAN,
        franchiseId: undefined,
        warehouseId: "ckwarehouse1234567890abcde1",
        isPublished: false,
        hasVariation: true,
        productImages: [
          {
            path: "products/existing-main.png",
            alt: "Existing main",
            position: 0,
          },
          {
            path: "products/new-detail.png",
            alt: "New detail",
            position: 1,
          },
        ],
        variants: [
          {
            id: EXISTING_VARIANT_ID,
            name: "Strawberry",
            sku: "POCKY-STRAWBERRY",
            costPrice: 120,
            sellingPrice: 180,
            stock: {
              quantity: 5,
            },
            position: 0,
            image: {
              path: "products/variants/existing-strawberry.png",
              alt: "Existing strawberry",
            },
          },
          {
            id: undefined,
            name: "Chocolate",
            sku: "POCKY-CHOCOLATE",
            costPrice: 110,
            sellingPrice: 170,
            stock: {
              quantity: 7,
            },
            position: 1,
            image: {
              path: "products/variants/new-chocolate.png",
              alt: "New chocolate",
            },
          },
        ],
      },
      expect.objectContaining({
        id: "product_123",
      }),
    );
    expect(removeProductImagesMock).not.toHaveBeenCalled();
  });

  it("cleans up newly uploaded images when the update fails after upload", async () => {
    uploadProductImagesMock
      .mockResolvedValueOnce({
        path: "products/new-detail.png",
      })
      .mockResolvedValueOnce({
        path: "products/variants/new-chocolate.png",
      });
    updateProductMock.mockRejectedValueOnce(new Error("database down"));

    await expect(
      updateProductService("product_123", createMultipartFormData({})),
    ).rejects.toThrow("database down");

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/new-detail.png",
      "products/variants/new-chocolate.png",
    ]);
  });

  it("cleans up archived variant images after a successful update", async () => {
    uploadProductImagesMock.mockResolvedValueOnce({
      path: "products/new-detail.png",
    });
    findProductByIdMock.mockResolvedValueOnce({
      id: "product_123",
      hasVariation: true,
      variants: [
        {
          id: EXISTING_VARIANT_ID,
          stocks: [
            {
              id: "stock_1",
              variantId: EXISTING_VARIANT_ID,
              warehouseId: "ckwarehouse1234567890abcde1",
              quantity: 4,
              warehouse: {
                id: "ckwarehouse1234567890abcde1",
                name: "Main Warehouse",
              },
            },
          ],
          image: {
            path: "products/variants/existing-strawberry.png",
            alt: "Existing strawberry",
          },
        },
        {
          id: "ck1234567890abcde1234568",
          stocks: [
            {
              id: "stock_2",
              variantId: "ck1234567890abcde1234568",
              warehouseId: "ckwarehouse1234567890abcde1",
              quantity: 3,
              warehouse: {
                id: "ckwarehouse1234567890abcde1",
                name: "Main Warehouse",
              },
            },
          ],
          image: {
            path: "products/variants/existing-chocolate.png",
            alt: "Existing chocolate",
          },
        },
      ],
      productImages: [
        {
          path: "products/existing-main.png",
          alt: "Existing main",
          position: 0,
        },
      ],
    });
    updateProductMock.mockResolvedValueOnce({
      id: "product_123",
      name: "Pocky",
    });

    const payload = createBasePayload();
    payload.variants = [payload.variants[0]];

    await updateProductService(
      "product_123",
      createMultipartFormData({
        payload,
        productFiles: [createFile("product-1.png")],
        variantFiles: [],
      }),
    );

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/variants/existing-chocolate.png",
    ]);
  });

  it("rejects invalid multipart form data before any upload or update work starts", async () => {
    const formData = new FormData();

    await expect(
      updateProductService("product_123", formData),
    ).rejects.toMatchObject({
      code: "INVALID_MULTIPART_FORM_DATA",
      message: "Missing payload field",
      status: 400,
    } satisfies Partial<AppError>);
    expect(uploadProductImagesMock).not.toHaveBeenCalled();
    expect(updateProductMock).not.toHaveBeenCalled();
  });
});
