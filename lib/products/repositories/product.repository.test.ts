import { MovementType, SupplierCountry } from "@/app/generated/prisma/client";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";
import type { ProductWithRelations } from "@/lib/validation-schema/product-schema";

const prismaMock = vi.hoisted(() => ({
  warehouse: {
    findUnique: vi.fn(),
  },
  product: {
    create: vi.fn(),
    findUnique: vi.fn(),
  },
  $transaction: vi.fn(),
}));

vi.mock("@/lib/infrastructure/database/prisma", () => ({
  default: prismaMock,
}));

import { createProduct, updateProduct } from "./product.repository";

const createProductCommand = (
  overrides: Partial<CreateProductCommandData> = {},
): CreateProductCommandData => ({
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
  ...overrides,
});

const createUpdateCommand = (
  overrides: Partial<UpdateProductCommandData> = {},
): UpdateProductCommandData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
  hasVariation: true,
  productImages: [
    {
      path: "products/main.png",
      alt: "Front angle",
      position: 0,
    },
  ],
  variants: [
    {
      id: "variant_1",
      name: "Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      stock: {
        quantity: 5,
      },
      position: 0,
      image: {
        path: "products/variants/strawberry.png",
        alt: "Variant front",
      },
    },
  ],
  ...overrides,
});

const createExistingProduct = (): ProductWithRelations =>
  ({
    id: "product_123",
    name: "Pocky",
    description: "Chocolate biscuit sticks",
    createdAt: new Date(),
    updatedAt: new Date(),
    franchiseId: null,
    supplierCountry: SupplierCountry.JAPAN,
    isPublished: false,
    hasVariation: true,
    productImages: [
      {
        id: "image_1",
        productId: "product_123",
        path: "products/main.png",
        alt: "Front angle",
        position: 0,
        isCover: false,
      },
    ],
    variants: [
      {
        id: "variant_1",
        productId: "product_123",
        sku: "POCKY-STRAWBERRY",
        createdAt: new Date(),
        updatedAt: new Date(),
        name: "Strawberry",
        costPrice: 120,
        sellingPrice: 180,
        position: 0,
        image: {
          id: "variant_image_1",
          variantId: "variant_1",
          path: "products/variants/strawberry.png",
          alt: "Variant front",
        },
        stocks: [
          {
            id: "stock_1",
            variantId: "variant_1",
            warehouseId: "ckwarehouse1234567890abcde1",
            quantity: 2,
            warehouse: {
              id: "ckwarehouse1234567890abcde1",
              name: "Main Warehouse",
            },
          },
        ],
      },
    ],
  }) as ProductWithRelations;

describe("product.repository", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    prismaMock.warehouse.findUnique.mockResolvedValue({
      id: "ckwarehouse1234567890abcde1",
    });
  });

  it("creates variant stock and creation movement", async () => {
    prismaMock.product.create.mockResolvedValue({
      id: "product_123",
    });

    await createProduct(createProductCommand());

    expect(prismaMock.product.create).toHaveBeenCalledWith(
      expect.objectContaining({
        data: expect.objectContaining({
          variants: {
            create: [
              expect.objectContaining({
                stocks: {
                  create: {
                    warehouseId: "ckwarehouse1234567890abcde1",
                    quantity: 12,
                  },
                },
                stockMovements: {
                  create: expect.objectContaining({
                    warehouseId: "ckwarehouse1234567890abcde1",
                    type: MovementType.IN,
                    quantity: 12,
                    note: "Product creation",
                    variantSku: "POCKY-BASE",
                    variantName: "Pocky",
                  }),
                },
              }),
            ],
          },
        }),
      }),
    );
  });

  it("writes an ADJUST movement when stock changes in the same warehouse", async () => {
    const txMock = {
      stock: {
        deleteMany: vi.fn(),
        upsert: vi.fn(),
      },
      productVariantImage: {
        deleteMany: vi.fn(),
      },
      productVariant: {
        deleteMany: vi.fn(),
        update: vi.fn(),
        create: vi.fn(),
      },
      product: {
        update: vi.fn(),
        findUnique: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
      },
      stockMovement: {
        create: vi.fn(),
        createMany: vi.fn(),
      },
    };
    prismaMock.$transaction.mockImplementation(async (callback) => callback(txMock));

    await updateProduct(
      "product_123",
      createUpdateCommand(),
      createExistingProduct(),
    );

    expect(txMock.stock.upsert).toHaveBeenCalledWith(
      expect.objectContaining({
        update: {
          quantity: 5,
        },
      }),
    );
    expect(txMock.stockMovement.create).toHaveBeenCalledWith({
      data: {
        variantId: "variant_1",
        variantSku: "POCKY-STRAWBERRY",
        variantName: "Strawberry",
        warehouseId: "ckwarehouse1234567890abcde1",
        type: MovementType.ADJUST,
        quantity: 3,
        note: "Product update",
      },
    });
  });

  it("writes OUT and IN movements when warehouse changes", async () => {
    const txMock = {
      stock: {
        deleteMany: vi.fn(),
        upsert: vi.fn(),
      },
      productVariantImage: {
        deleteMany: vi.fn(),
      },
      productVariant: {
        deleteMany: vi.fn(),
        update: vi.fn(),
        create: vi.fn(),
      },
      product: {
        update: vi.fn(),
        findUnique: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
      },
      stockMovement: {
        create: vi.fn(),
        createMany: vi.fn(),
      },
    };
    prismaMock.warehouse.findUnique.mockResolvedValue({
      id: "ckwarehouse9876543210abcde1",
    });
    prismaMock.$transaction.mockImplementation(async (callback) => callback(txMock));

    await updateProduct(
      "product_123",
      createUpdateCommand({
        warehouseId: "ckwarehouse9876543210abcde1",
      }),
      createExistingProduct(),
    );

    expect(txMock.stock.deleteMany).toHaveBeenCalledWith({
      where: {
        variantId: "variant_1",
      },
    });
    expect(txMock.stockMovement.createMany).toHaveBeenCalledWith({
      data: [
        {
          variantId: "variant_1",
          variantSku: "POCKY-STRAWBERRY",
          variantName: "Strawberry",
          warehouseId: "ckwarehouse1234567890abcde1",
          type: MovementType.OUT,
          quantity: 2,
          note: "Warehouse transfer from product update",
        },
        {
          variantId: "variant_1",
          variantSku: "POCKY-STRAWBERRY",
          variantName: "Strawberry",
          warehouseId: "ckwarehouse9876543210abcde1",
          type: MovementType.IN,
          quantity: 5,
          note: "Warehouse transfer from product update",
        },
      ],
    });
  });
});
