import { AppError } from "@/core/errors/errors";
import { SupplierCountry } from "@/app/generated/prisma/enums";
import { beforeEach, describe, expect, it, vi } from "vitest";

const { createProductMock, removeProductImagesMock, uploadProductImagesMock } =
  vi.hoisted(() => ({
    createProductMock: vi.fn(),
    uploadProductImagesMock: vi.fn(),
    removeProductImagesMock: vi.fn(),
  }));

vi.mock("@/lib/products/repositories/product.repository", () => ({
  createProduct: createProductMock,
}));

vi.mock("@/lib/infrastructure/storage/server", () => ({
  uploadProductImages: uploadProductImagesMock,
  removeProductImages: removeProductImagesMock,
}));

import { createProductService } from "./create-product.service";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createBasePayload = () => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
  hasVariation: false,
  baseSku: "POCKY-BASE",
  baseCostPrice: 100,
  baseSellingPrice: 150,
  baseStock: {
    quantity: 12,
  },
  variants: [],
  productImages: [
    {
      alt: "Front angle",
      position: 0,
    },
  ],
});

const createMultipartFormData = ({
  payload = createBasePayload(),
  productFiles = [createFile("product-1.png")],
  variantFiles = [] as Array<{ clientKey: string; file: File }>,
}: {
  payload?: Record<string, unknown>;
  productFiles?: File[];
  variantFiles?: Array<{ clientKey: string; file: File }>;
}) => {
  const formData = new FormData();

  formData.set("payload", JSON.stringify(payload));

  for (const file of productFiles) {
    formData.append("images", file);
  }

  for (const variantFile of variantFiles) {
    formData.append(
      `variantImages:${variantFile.clientKey}`,
      variantFile.file,
    );
  }

  return formData;
};

describe("createProductService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("creates a product from multipart form data using the validated payload and uploaded images", async () => {
    uploadProductImagesMock.mockResolvedValueOnce({
      path: "products/main.png",
    });
    createProductMock.mockResolvedValueOnce({
      id: "product_123",
      name: "Pocky",
    });

    const result = await createProductService(createMultipartFormData({}));

    expect(result).toEqual({
      id: "product_123",
      name: "Pocky",
    });
    expect(uploadProductImagesMock).toHaveBeenCalledTimes(1);
    expect(createProductMock).toHaveBeenCalledWith({
      name: "Pocky",
      description: "Chocolate biscuit sticks",
      supplierCountry: SupplierCountry.JAPAN,
      franchiseId: undefined,
      warehouseId: "ckwarehouse1234567890abcde1",
      isPublished: false,
      hasVariation: false,
      productImages: [
        {
          path: "products/main.png",
          alt: "Front angle",
          position: 0,
        },
      ],
      variants: [
        {
          name: "Pocky",
          sku: "POCKY-BASE",
          costPrice: 100,
          sellingPrice: 150,
          stock: {
            quantity: 12,
          },
          position: 0,
          image: {
            path: "products/main.png",
            alt: "Front angle",
          },
        },
      ],
    });
    expect(removeProductImagesMock).not.toHaveBeenCalled();
  });

  it("rejects invalid multipart form data before any upload or persistence work starts", async () => {
    const formData = new FormData();

    await expect(createProductService(formData)).rejects.toMatchObject({
      code: "INVALID_MULTIPART_FORM_DATA",
      message: "Missing payload field",
      status: 400,
    } satisfies Partial<AppError>);
    expect(uploadProductImagesMock).not.toHaveBeenCalled();
    expect(createProductMock).not.toHaveBeenCalled();
  });

  it("cleans up uploaded images when persistence fails after uploads complete", async () => {
    uploadProductImagesMock
      .mockResolvedValueOnce({
        path: "products/main.png",
      })
      .mockResolvedValueOnce({
        path: "products/variants/strawberry.png",
      });
    createProductMock.mockRejectedValueOnce(new Error("database down"));

    const formData = createMultipartFormData({
      payload: {
        ...createBasePayload(),
        hasVariation: true,
        baseSku: undefined,
        baseCostPrice: undefined,
        baseSellingPrice: undefined,
        baseStock: undefined,
        variants: [
          {
            clientKey: "variant-strawberry",
            name: "Strawberry",
            sku: "POCKY-STRAWBERRY",
            costPrice: 120,
            sellingPrice: 180,
            stock: {
              quantity: 5,
            },
            position: 0,
            image: {
              alt: "Variant front",
            },
          },
        ],
      },
      variantFiles: [
        {
          clientKey: "variant-strawberry",
          file: createFile("variant-1.png"),
        },
      ],
    });

    await expect(createProductService(formData)).rejects.toThrow(
      "database down",
    );

    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/main.png",
      "products/variants/strawberry.png",
    ]);
  });
});
