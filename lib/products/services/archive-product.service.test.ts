import { beforeEach, describe, expect, it, vi } from "vitest";

const {
  archiveProductMock,
  findProductByIdMock,
  removeProductImagesMock,
} = vi.hoisted(() => ({
  archiveProductMock: vi.fn(),
  findProductByIdMock: vi.fn(),
  removeProductImagesMock: vi.fn(),
}));

vi.mock("@/lib/products/repositories/product.repository", () => ({
  archiveProduct: archiveProductMock,
  findProductById: findProductByIdMock,
}));

vi.mock("@/lib/infrastructure/storage/server", () => ({
  removeProductImages: removeProductImagesMock,
}));

import { archiveProductService } from "./archive-product.service";

describe("archiveProductService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("archives the product and removes product and variant images from storage", async () => {
    findProductByIdMock.mockResolvedValue({
      id: "product_123",
      productImages: [
        {
          path: "products/main.png",
        },
      ],
      variants: [
        {
          id: "variant_1",
          image: {
            path: "products/variants/strawberry.png",
          },
        },
      ],
    });
    archiveProductMock.mockResolvedValue({
      id: "product_123",
      isArchived: true,
    });

    const result = await archiveProductService("product_123");

    expect(findProductByIdMock).toHaveBeenCalledWith("product_123", true);
    expect(archiveProductMock).toHaveBeenCalledWith("product_123");
    expect(removeProductImagesMock).toHaveBeenCalledTimes(2);
    expect(removeProductImagesMock).toHaveBeenNthCalledWith(1, [
      "products/main.png",
    ]);
    expect(removeProductImagesMock).toHaveBeenNthCalledWith(2, [
      "products/variants/strawberry.png",
    ]);
    expect(result).toEqual({
      id: "product_123",
      isArchived: true,
    });
  });

  it("dedupes shared product and variant image paths during cleanup", async () => {
    findProductByIdMock.mockResolvedValue({
      id: "product_123",
      productImages: [
        {
          path: "products/shared.png",
        },
      ],
      variants: [
        {
          id: "variant_1",
          image: {
            path: "products/shared.png",
          },
        },
      ],
    });
    archiveProductMock.mockResolvedValue({
      id: "product_123",
      isArchived: true,
    });

    await archiveProductService("product_123");

    expect(removeProductImagesMock).toHaveBeenCalledTimes(1);
    expect(removeProductImagesMock).toHaveBeenCalledWith([
      "products/shared.png",
    ]);
  });

  it("throws when image cleanup fails after archiving", async () => {
    findProductByIdMock.mockResolvedValue({
      id: "product_123",
      productImages: [
        {
          path: "products/main.png",
        },
      ],
      variants: [],
    });
    archiveProductMock.mockResolvedValue({
      id: "product_123",
      isArchived: true,
    });
    removeProductImagesMock.mockRejectedValueOnce(new Error("storage down"));

    await expect(archiveProductService("product_123")).rejects.toMatchObject({
      code: "ARCHIVE_PRODUCT_IMAGE_CLEANUP_FAILED",
      message: "Product archived but image cleanup failed",
      status: 500,
    });
  });
});
