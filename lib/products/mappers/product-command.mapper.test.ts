import { SupplierCountry } from "@/app/generated/prisma/enums";
import type {
  CreateProductFormData,
  ProductWithRelations,
} from "@/lib/products/schema";
import { describe, expect, it } from "vitest";
import {
  toCreateProductCommand,
  toUpdateProductCommandInput,
} from "./product-command.mapper";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createFormData = (
  overrides: Partial<CreateProductFormData> = {},
): CreateProductFormData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
  hasVariation: false,
  baseSku: "POCKY-BASE",
  baseCostPrice: 100,
  baseSellingPrice: 150,
  baseStock: {
    quantity: 12,
  },
  variations: [],
  variants: [],
  productImages: [
    {
      file: createFile("product-1.png"),
      alt: "Front angle",
      position: 0,
    },
  ],
  ...overrides,
});

describe("product-command.mapper", () => {
  it("creates a single fallback variant from the first uploaded product image when variations are disabled", () => {
    const command = toCreateProductCommand(createFormData(), [
      {
        path: "products/main.png",
        alt: "Front angle",
        position: 0,
      },
    ]);

    expect(command).toEqual({
      name: "Pocky",
      description: "Chocolate biscuit sticks",
      supplierCountry: SupplierCountry.JAPAN,
      franchiseId: undefined,
      warehouseId: "ckwarehouse1234567890abcde1",
      isPublished: false,
      hasVariation: false,
      variations: [],
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
          selectedOptions: [],
        },
      ],
    });
  });

  it("maps explicit variations and generated variant names when variations are enabled", () => {
    const command = toCreateProductCommand(
      createFormData({
        hasVariation: true,
        baseSku: undefined,
        baseCostPrice: undefined,
        baseSellingPrice: undefined,
        baseStock: undefined,
        variations: [
          {
            id: "variation-color",
            name: "Color",
            position: 0,
            options: [
              {
                id: "option-red",
                value: "Red",
                position: 0,
              },
            ],
          },
        ],
        variants: [
          {
            clientKey: "variant-red",
            name: "ignored",
            sku: "POCKY-RED",
            costPrice: 120,
            sellingPrice: 180,
            stock: {
              quantity: 5,
            },
            position: 3,
            selectedOptions: [
              {
                variationId: "variation-color",
                optionId: "option-red",
              },
            ],
            image: {
              file: createFile("variant-1.png"),
              alt: "Variant front",
            },
          },
        ],
      }),
      [
        {
          path: "products/main.png",
          alt: "Front angle",
          position: 0,
        },
      ],
      new Map([
        [
          "variant-red",
          {
            path: "products/variants/red.png",
            alt: "Variant front",
          },
        ],
      ]),
    );

    expect(command.variations).toEqual([
      {
        id: "variation-color",
        name: "Color",
        position: 0,
        options: [
          {
            id: "option-red",
            value: "Red",
            position: 0,
          },
        ],
      },
    ]);
    expect(command.variants).toEqual([
      {
        name: "Color: Red",
        sku: "POCKY-RED",
        costPrice: 120,
        sellingPrice: 180,
        stock: {
          quantity: 5,
        },
        position: 3,
        selectedOptions: [
          {
            variationId: "variation-color",
            optionId: "option-red",
          },
        ],
        image: {
          path: "products/variants/red.png",
          alt: "Variant front",
        },
      },
    ]);
  });

  it("maps uploaded variant images by stable client key instead of map order", () => {
    const command = toCreateProductCommand(
      createFormData({
        hasVariation: true,
        baseSku: undefined,
        baseCostPrice: undefined,
        baseSellingPrice: undefined,
        baseStock: undefined,
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
              {
                id: "option-matcha",
                value: "Matcha",
                position: 1,
              },
            ],
          },
        ],
        variants: [
          {
            clientKey: "variant-strawberry",
            name: "ignored",
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
              file: createFile("variant-1.png"),
              alt: "Strawberry front",
            },
          },
          {
            clientKey: "variant-matcha",
            name: "ignored",
            sku: "POCKY-MATCHA",
            costPrice: 130,
            sellingPrice: 190,
            stock: {
              quantity: 7,
            },
            position: 1,
            selectedOptions: [
              {
                variationId: "variation-flavor",
                optionId: "option-matcha",
              },
            ],
            image: {
              file: createFile("variant-2.png"),
              alt: "Matcha front",
            },
          },
        ],
      }),
      [
        {
          path: "products/main.png",
          alt: "Front angle",
          position: 0,
        },
      ],
      new Map([
        [
          "variant-matcha",
          {
            path: "products/variants/matcha.png",
            alt: "Matcha front",
          },
        ],
        [
          "variant-strawberry",
          {
            path: "products/variants/strawberry.png",
            alt: "Strawberry front",
          },
        ],
      ]),
    );

    expect(command.variants).toEqual([
      {
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
          alt: "Strawberry front",
        },
      },
      {
        name: "Flavor: Matcha",
        sku: "POCKY-MATCHA",
        costPrice: 130,
        sellingPrice: 190,
        stock: {
          quantity: 7,
        },
        position: 1,
        selectedOptions: [
          {
            variationId: "variation-flavor",
            optionId: "option-matcha",
          },
        ],
        image: {
          path: "products/variants/matcha.png",
          alt: "Matcha front",
        },
      },
    ]);
  });

  it("throws when variation products do not receive all uploaded variant images", () => {
    expect(() =>
      toCreateProductCommand(
        createFormData({
          hasVariation: true,
          baseSku: undefined,
          baseCostPrice: undefined,
          baseSellingPrice: undefined,
          baseStock: undefined,
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
          variants: [
            {
              clientKey: "variant-strawberry",
              name: "ignored",
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
                file: createFile("variant-1.png"),
                alt: "Variant front",
              },
            },
          ],
        }),
        [
          {
            path: "products/main.png",
            alt: "Front angle",
            position: 0,
          },
        ],
        new Map(),
      ),
    ).toThrow("Variant images are required for every variant.");
  });

  it("hydrates update form input with explicit variation relations", () => {
    const product = {
      id: "product_123",
      name: "Pocky",
      description: "Chocolate biscuit sticks",
      supplierCountry: SupplierCountry.JAPAN,
      franchiseId: null,
      isPublished: false,
      hasVariation: true,
      createdAt: new Date(),
      updatedAt: new Date(),
      sellMode: "IN_STOCK",
      preorderReleaseAt: null,
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
          id: "variation-color-db",
          productId: "product_123",
          name: "Color",
          position: 0,
          createdAt: new Date(),
          updatedAt: new Date(),
          options: [
            {
              id: "option-red-db",
              variationId: "variation-color-db",
              value: "Red",
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
          sku: "POCKY-RED",
          createdAt: new Date(),
          updatedAt: new Date(),
          name: "Color: Red",
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
            path: "products/variants/red.png",
            alt: "Red front",
          },
          variationOptions: [
            {
              productVariantId: "variant_1",
              variationId: "variation-color-db",
              variationOptionId: "option-red-db",
              variation: {
                id: "variation-color-db",
                productId: "product_123",
                name: "Color",
                position: 0,
                createdAt: new Date(),
                updatedAt: new Date(),
              },
              variationOption: {
                id: "option-red-db",
                variationId: "variation-color-db",
                value: "Red",
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
              quantity: 5,
              warehouse: {
                id: "ckwarehouse1234567890abcde1",
                name: "Main Warehouse",
              },
            },
          ],
        },
      ],
    } as ProductWithRelations;

    expect(toUpdateProductCommandInput(product)).toEqual({
      name: "Pocky",
      description: "Chocolate biscuit sticks",
      supplierCountry: SupplierCountry.JAPAN,
      franchiseId: undefined,
      warehouseId: "ckwarehouse1234567890abcde1",
      hasVariation: true,
      baseVariantId: undefined,
      baseSku: undefined,
      baseCostPrice: undefined,
      baseSellingPrice: undefined,
      baseStock: undefined,
      variations: [
        {
          id: "variation-color-db",
          name: "Color",
          position: 0,
          options: [
            {
              id: "option-red-db",
              value: "Red",
              position: 0,
            },
          ],
        },
      ],
      variants: [
        {
          id: "variant_1",
          name: "Color: Red",
          sku: "POCKY-RED",
          costPrice: 120,
          sellingPrice: 180,
          stock: {
            quantity: 5,
          },
          position: 0,
          selectedOptions: [
            {
              variationId: "variation-color-db",
              optionId: "option-red-db",
            },
          ],
          newImage: undefined,
          existImage: {
            path: "products/variants/red.png",
            alt: "Red front",
          },
        },
      ],
      newProductImages: [],
      existProductImages: [
        {
          path: "products/main.png",
          alt: "Front angle",
          position: 0,
        },
      ],
    });
  });

  it("keeps legacy variation products editable by leaving explicit variations empty when relations do not exist", () => {
    const product = {
      id: "product_legacy",
      name: "Pocky",
      description: "Chocolate biscuit sticks",
      supplierCountry: SupplierCountry.JAPAN,
      franchiseId: null,
      isPublished: false,
      hasVariation: true,
      createdAt: new Date(),
      updatedAt: new Date(),
      sellMode: "IN_STOCK",
      preorderReleaseAt: null,
      isArchived: false,
      productImages: [
        {
          id: "image_1",
          productId: "product_legacy",
          path: "products/main.png",
          alt: "Front angle",
          position: 0,
          isCover: false,
        },
      ],
      variations: [],
      variants: [
        {
          id: "variant_1",
          productId: "product_legacy",
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
            alt: "Strawberry front",
          },
          variationOptions: [],
          stocks: [
            {
              id: "stock_1",
              variantId: "variant_1",
              warehouseId: "ckwarehouse1234567890abcde1",
              quantity: 5,
              warehouse: {
                id: "ckwarehouse1234567890abcde1",
                name: "Main Warehouse",
              },
            },
          ],
        },
      ],
    } as ProductWithRelations;

    expect(toUpdateProductCommandInput(product)).toMatchObject({
      hasVariation: true,
      variations: [],
      variants: [
        {
          id: "variant_1",
          name: "Flavor: Strawberry",
          selectedOptions: [],
        },
      ],
    });
  });
});
