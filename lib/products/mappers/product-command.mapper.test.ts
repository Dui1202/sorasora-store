import { SupplierCountry } from "@/app/generated/prisma/enums";
import type { CreateProductFormData } from "@/lib/validation-schema/product-schema";
import { describe, expect, it } from "vitest";
import { toCreateProductCommand } from "./product-command.mapper";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createFormData = (
  overrides: Partial<CreateProductFormData> = {},
): CreateProductFormData => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  isPublished: false,
  hasVariation: false,
  baseSku: "POCKY-BASE",
  baseCostPrice: 100,
  baseSellingPrice: 150,
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
            name: "Strawberry",
            sku: "POCKY-STRAWBERRY",
            costPrice: 120,
            sellingPrice: 180,
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
      [
        {
          path: "products/variants/strawberry.png",
          alt: "Variant front",
        },
      ],
    );

    expect(command.variants).toEqual([
      {
        name: "Strawberry",
        sku: "POCKY-STRAWBERRY",
        costPrice: 120,
        sellingPrice: 180,
        position: 3,
        image: {
          path: "products/variants/strawberry.png",
          alt: "Variant front",
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
        }),
        [
          {
            path: "products/main.png",
            alt: "Front angle",
            position: 0,
          },
        ],
        [],
      ),
    ).toThrow("Variant images are required for every variant.");
  });
});
