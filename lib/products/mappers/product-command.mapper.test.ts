import { SupplierCountry } from "@/app/generated/prisma/enums";
import type {
  CreateProductFormData,
  ProductWithRelations,
} from "@/lib/validation-schema/product-schema";
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
  });

  it("maps uploaded variant images by order when variations are enabled", () => {
    const command = toCreateProductCommand(
      createFormData({
        hasVariation: true,
        baseSku: undefined,
        baseCostPrice: undefined,
        baseSellingPrice: undefined,
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
            position: 3,
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
          "variant-strawberry",
          {
            path: "products/variants/strawberry.png",
            alt: "Variant front",
          },
        ],
      ]),
    );

    expect(command.variants).toEqual([
      {
        name: "Strawberry",
        sku: "POCKY-STRAWBERRY",
        costPrice: 120,
        sellingPrice: 180,
        stock: {
          quantity: 5,
        },
        position: 3,
        image: {
          path: "products/variants/strawberry.png",
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
              file: createFile("variant-1.png"),
              alt: "Strawberry front",
            },
          },
          {
            clientKey: "variant-matcha",
            name: "Matcha",
            sku: "POCKY-MATCHA",
            costPrice: 130,
            sellingPrice: 190,
            stock: {
              quantity: 7,
            },
            position: 1,
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
          alt: "Strawberry front",
        },
      },
      {
        name: "Matcha",
        sku: "POCKY-MATCHA",
        costPrice: 130,
        sellingPrice: 190,
        stock: {
          quantity: 7,
        },
        position: 1,
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

  it("hydrates update form input with shared warehouse and stock quantities", () => {
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
          name: "Flavor: Strawberry",
          costPrice: 120,
          sellingPrice: 180,
          position: 0,
          image: {
            id: "variant_image_1",
            variantId: "variant_1",
            path: "products/variants/strawberry.png",
            alt: "Strawberry front",
          },
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
          newImage: undefined,
          existImage: {
            path: "products/variants/strawberry.png",
            alt: "Strawberry front",
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
});
