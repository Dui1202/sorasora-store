import { MovementType, SupplierCountry } from "@/app/generated/prisma/client";
import type {
  ProductWithRelations,
} from "@/lib/products/schema";
import { beforeEach, describe, expect, it, vi } from "vitest";
import type {
  CreateProductCommandData,
  UpdateProductCommandData,
} from "../mappers/product-command.mapper";

const prismaMock = vi.hoisted(() => ({
  warehouse: {
    findUnique: vi.fn(),
  },
  product: {
    create: vi.fn(),
    findFirst: vi.fn(),
    findMany: vi.fn(),
    update: vi.fn(),
  },
  variation: {
    create: vi.fn(),
    deleteMany: vi.fn(),
  },
  $transaction: vi.fn(),
}));

vi.mock("@/lib/infrastructure/database/prisma", () => ({
  default: prismaMock,
}));

import {
  archiveProduct,
  createProduct,
  findProductById,
  findProducts,
  updateProduct,
} from "./product.repository";

const createProductCommand = (
  overrides: Partial<CreateProductCommandData> = {},
): CreateProductCommandData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
  hasVariation: true,
  variations: [
    {
      id: "variation-flavor",
      name: "Flavor",
      position: 0,
      options: [
        {
          id: "option-strawberry",
          value: "Strawberry",
          position: 0,
        },
      ],
    },
  ],
  productImages: [
    {
      path: "products/main.png",
      alt: "Front angle",
      position: 0,
    },
  ],
  variants: [
    {
      name: "Flavor: Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      stock: {
        quantity: 12,
      },
      position: 0,
      selectedOptions: [
        {
          variationId: "variation-flavor",
          optionId: "option-strawberry",
        },
      ],
      image: {
        path: "products/variants/strawberry.png",
        alt: "Variant front",
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
  variations: [
    {
      id: "variation-flavor",
      name: "Flavor",
      position: 0,
      options: [
        {
          id: "option-strawberry",
          value: "Strawberry",
          position: 0,
        },
      ],
    },
  ],
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
      name: "Flavor: Strawberry",
      sku: "POCKY-STRAWBERRY",
      costPrice: 120,
      sellingPrice: 180,
      stock: {
        quantity: 5,
      },
      position: 0,
      selectedOptions: [
        {
          variationId: "variation-flavor",
          optionId: "option-strawberry",
        },
      ],
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
    sellMode: "IN_STOCK",
    preorderReleaseAt: null,
    isPublished: false,
    hasVariation: true,
    isArchived: false,
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
    variations: [
      {
        id: "variation_db_1",
        productId: "product_123",
        name: "Flavor",
        position: 0,
        createdAt: new Date(),
        updatedAt: new Date(),
        options: [
          {
            id: "variation_option_db_1",
            variationId: "variation_db_1",
            value: "Strawberry",
            position: 0,
            createdAt: new Date(),
            updatedAt: new Date(),
          },
        ],
      },
    ],
    variants: [
      {
        id: "variant_1",
        productId: "product_123",
        sku: "POCKY-STRAWBERRY",
        createdAt: new Date(),
        updatedAt: new Date(),
        name: "Flavor: Strawberry",
        costPrice: 120,
        sellingPrice: 180,
        position: 0,
        isArchived: false,
        allowFullPayment: true,
        allowDeposit: false,
        depositPrice: null,
        image: {
          id: "variant_image_1",
          variantId: "variant_1",
          path: "products/variants/strawberry.png",
          alt: "Variant front",
        },
        variationOptions: [
          {
            productVariantId: "variant_1",
            variationId: "variation_db_1",
            variationOptionId: "variation_option_db_1",
            variation: {
              id: "variation_db_1",
              productId: "product_123",
              name: "Flavor",
              position: 0,
              createdAt: new Date(),
              updatedAt: new Date(),
            },
            variationOption: {
              id: "variation_option_db_1",
              variationId: "variation_db_1",
              value: "Strawberry",
              position: 0,
              createdAt: new Date(),
              updatedAt: new Date(),
            },
          },
        ],
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

  it("filters archived products and includes explicit variation relations in list queries", async () => {
    prismaMock.product.findMany.mockResolvedValue([]);

    await findProducts();

    expect(prismaMock.product.findMany).toHaveBeenCalledWith({
      where: {
        isArchived: false,
      },
      include: {
        productImages: true,
        variations: {
          orderBy: {
            position: "asc",
          },
          include: {
            options: {
              orderBy: {
                position: "asc",
              },
            },
          },
        },
        variants: {
          where: {
            isArchived: false,
          },
          orderBy: {
            position: "asc",
          },
          include: {
            image: true,
            variationOptions: {
              include: {
                variation: true,
                variationOption: true,
              },
            },
            stocks: {
              include: {
                warehouse: true,
              },
            },
          },
        },
      },
    });
  });

  it("filters archived products and includes explicit variation relations in detail queries", async () => {
    prismaMock.product.findFirst.mockResolvedValue(null);

    await findProductById("product_123", true);

    expect(prismaMock.product.findFirst).toHaveBeenCalledWith({
      where: {
        id: "product_123",
        isArchived: false,
      },
      include: {
        productImages: true,
        variations: {
          orderBy: {
            position: "asc",
          },
          include: {
            options: {
              orderBy: {
                position: "asc",
              },
            },
          },
        },
        variants: {
          where: {
            isArchived: false,
          },
          orderBy: {
            position: "asc",
          },
          include: {
            image: true,
            variationOptions: {
              include: {
                variation: true,
                variationOption: true,
              },
            },
            stocks: {
              include: {
                warehouse: true,
              },
            },
          },
        },
      },
    });
  });

  it("creates variation structure and variant-option links for variation products", async () => {
    const txMock = {
      product: {
        create: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
        findFirst: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
      },
      variation: {
        create: vi.fn().mockResolvedValue({
          id: "variation_db_1",
          options: [
            {
              id: "variation_option_db_1",
              value: "Strawberry",
              position: 0,
            },
          ],
        }),
      },
      productVariant: {
        create: vi.fn().mockResolvedValue({
          id: "variant_1",
        }),
      },
      productVariantOption: {
        createMany: vi.fn(),
      },
      stock: {
        upsert: vi.fn(),
      },
      stockMovement: {
        create: vi.fn(),
      },
    };
    prismaMock.$transaction.mockImplementation(async (callback) =>
      callback(txMock),
    );

    await createProduct(createProductCommand());

    expect(txMock.variation.create).toHaveBeenCalledWith({
      data: {
        productId: "product_123",
        name: "Flavor",
        position: 0,
        options: {
          create: [
            {
              value: "Strawberry",
              position: 0,
            },
          ],
        },
      },
      include: {
        options: true,
      },
    });
    expect(txMock.productVariantOption.createMany).toHaveBeenCalledWith({
      data: [
        {
          productVariantId: "variant_1",
          variationId: "variation_db_1",
          variationOptionId: "variation_option_db_1",
        },
      ],
    });
    expect(txMock.stockMovement.create).toHaveBeenCalledWith({
      data: {
        variantId: "variant_1",
        variantSku: "POCKY-STRAWBERRY",
        variantName: "Flavor: Strawberry",
        warehouseId: "ckwarehouse1234567890abcde1",
        type: MovementType.IN,
        quantity: 12,
        note: "Product creation",
      },
    });
  });

  it("archives the product and related active variants instead of deleting them", async () => {
    const txMock = {
      stock: {
        deleteMany: vi.fn(),
      },
      productImage: {
        deleteMany: vi.fn(),
      },
      productVariant: {
        findMany: vi
          .fn()
          .mockResolvedValue([{ id: "variant_1" }, { id: "variant_2" }]),
        updateMany: vi.fn(),
      },
      productVariantImage: {
        deleteMany: vi.fn(),
      },
      product: {
        update: vi.fn().mockResolvedValue({
          id: "product_123",
          isArchived: true,
        }),
      },
    };
    prismaMock.$transaction.mockImplementation(async (callback) =>
      callback(txMock),
    );

    const result = await archiveProduct("product_123");

    expect(txMock.productVariant.updateMany).toHaveBeenCalledWith({
      where: {
        productId: "product_123",
        id: {
          in: ["variant_1", "variant_2"],
        },
      },
      data: {
        isArchived: true,
      },
    });
    expect(result).toEqual({
      id: "product_123",
      isArchived: true,
    });
  });

  it("rejects submitted variant ids that are no longer active on the product", async () => {
    await expect(
      updateProduct(
        "product_123",
        createUpdateCommand({
          variants: [
            {
              id: "variant_archived",
              name: "Flavor: Archived",
              sku: "POCKY-ARCHIVED",
              costPrice: 100,
              sellingPrice: 150,
              stock: {
                quantity: 1,
              },
              position: 0,
              selectedOptions: [
                {
                  variationId: "variation-flavor",
                  optionId: "option-strawberry",
                },
              ],
              image: {
                path: "products/variants/archived.png",
                alt: "Archived",
              },
            },
          ],
        }),
        createExistingProduct(),
      ),
    ).rejects.toMatchObject({
      code: "UPDATE_PRODUCT_FAILED",
      message: "Variant does not belong to product",
      status: 400,
    });
  });

  it("replaces existing variation structure and recreates variant selection links on update", async () => {
    const txMock = {
      stock: {
        deleteMany: vi.fn(),
        upsert: vi.fn(),
      },
      productVariantImage: {
        deleteMany: vi.fn(),
      },
      productVariantOption: {
        deleteMany: vi.fn(),
        createMany: vi.fn(),
      },
      productVariant: {
        updateMany: vi.fn(),
        update: vi.fn(),
        create: vi.fn(),
      },
      product: {
        update: vi.fn(),
        findFirst: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
      },
      variation: {
        deleteMany: vi.fn(),
        create: vi.fn().mockResolvedValue({
          id: "variation_db_2",
          options: [
            {
              id: "variation_option_db_2",
              value: "Strawberry",
              position: 0,
            },
          ],
        }),
      },
      stockMovement: {
        create: vi.fn(),
        createMany: vi.fn(),
      },
    };
    prismaMock.$transaction.mockImplementation(async (callback) =>
      callback(txMock),
    );

    await updateProduct(
      "product_123",
      createUpdateCommand(),
      createExistingProduct(),
    );

    expect(txMock.variation.deleteMany).toHaveBeenCalledWith({
      where: {
        productId: "product_123",
      },
    });
    expect(txMock.productVariant.update).toHaveBeenCalledWith({
      where: {
        id: "variant_1",
      },
      data: expect.objectContaining({
        name: "Flavor: Strawberry",
      }),
    });
    expect(txMock.productVariantOption.createMany).toHaveBeenCalledWith({
      data: [
        {
          productVariantId: "variant_1",
          variationId: "variation_db_2",
          variationOptionId: "variation_option_db_2",
        },
      ],
    });
  });

  it("removes variation records and links when switching a product back to a single variant", async () => {
    const txMock = {
      stock: {
        deleteMany: vi.fn(),
        upsert: vi.fn(),
      },
      productVariantImage: {
        deleteMany: vi.fn(),
      },
      productVariantOption: {
        deleteMany: vi.fn(),
        createMany: vi.fn(),
      },
      productVariant: {
        updateMany: vi.fn(),
        update: vi.fn(),
        create: vi.fn(),
      },
      product: {
        update: vi.fn(),
        findFirst: vi.fn().mockResolvedValue({
          id: "product_123",
        }),
      },
      variation: {
        deleteMany: vi.fn(),
        create: vi.fn(),
      },
      stockMovement: {
        create: vi.fn(),
        createMany: vi.fn(),
      },
    };
    prismaMock.$transaction.mockImplementation(async (callback) =>
      callback(txMock),
    );

    await updateProduct(
      "product_123",
      createUpdateCommand({
        hasVariation: false,
        variations: [],
        variants: [
          {
            id: "variant_1",
            name: "Pocky",
            sku: "POCKY-BASE",
            costPrice: 100,
            sellingPrice: 150,
            stock: {
              quantity: 5,
            },
            position: 0,
            selectedOptions: [],
            image: {
              path: "products/main.png",
              alt: "Front angle",
            },
          },
        ],
      }),
      {
        ...createExistingProduct(),
        hasVariation: false,
      },
    );

    expect(txMock.variation.deleteMany).toHaveBeenCalledWith({
      where: {
        productId: "product_123",
      },
    });
    expect(txMock.productVariantOption.createMany).not.toHaveBeenCalled();
  });
});
