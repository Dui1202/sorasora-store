import { SupplierCountry } from "@/lib/domain/products/supplier-country";
import { describe, expect, it } from "vitest";
import { createProductFormSchema } from "./create-product-form";
import { updateProductFormSchema } from "./update-product-form";

const createFile = (name: string, content = "image-bytes") =>
  new File([content], name, { type: "image/png" });

const createVariationFormData = () => ({
  name: "Pocky",
  description: "Chocolate biscuit sticks",
  supplierCountry: SupplierCountry.JAPAN,
  franchiseId: undefined,
  warehouseId: "ckwarehouse1234567890abcde1",
  isPublished: false,
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
        file: createFile("variant-1.png"),
        alt: "Strawberry front",
      },
    },
  ],
  productImages: [
    {
      file: createFile("product-1.png"),
      alt: "Front angle",
      position: 0,
    },
  ],
});

describe("product variation validation", () => {
  it("rejects duplicate variation names", () => {
    const result = createProductFormSchema.safeParse({
      ...createVariationFormData(),
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
        {
          id: "variation-style",
          name: " flavor ",
          position: 1,
          options: [
            {
              id: "option-classic",
              value: "Classic",
              position: 0,
            },
          ],
        },
      ],
      variants: [
        {
          ...createVariationFormData().variants[0],
          selectedOptions: [
            {
              variationId: "variation-flavor",
              optionId: "option-strawberry",
            },
            {
              variationId: "variation-style",
              optionId: "option-classic",
            },
          ],
        },
      ],
    });

    expect(result.success).toBe(false);
  });

  it("rejects duplicate option values within a variation", () => {
    const result = createProductFormSchema.safeParse({
      ...createVariationFormData(),
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
              id: "option-strawberry-2",
              value: " strawberry ",
              position: 1,
            },
          ],
        },
      ],
    });

    expect(result.success).toBe(false);
  });

  it("rejects variants missing explicit selections for every variation", () => {
    const result = createProductFormSchema.safeParse({
      ...createVariationFormData(),
      variations: [
        ...createVariationFormData().variations,
        {
          id: "variation-size",
          name: "Size",
          position: 1,
          options: [
            {
              id: "option-m",
              value: "M",
              position: 0,
            },
          ],
        },
      ],
    });

    expect(result.success).toBe(false);
  });

  it("rejects duplicate variant combinations during update", () => {
    const baseData = createVariationFormData();
    const result = updateProductFormSchema.safeParse({
      ...baseData,
      baseVariantId: undefined,
      existProductImages: [
        {
          path: "products/existing-main.png",
          alt: "Front angle",
          position: 0,
        },
      ],
      newProductImages: [],
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
          existImage: {
            path: "products/variants/strawberry.png",
            alt: "Strawberry front",
          },
          newImage: undefined,
        },
        {
          id: "variant_2",
          name: "Flavor: Strawberry",
          sku: "POCKY-STRAWBERRY-2",
          costPrice: 130,
          sellingPrice: 190,
          stock: {
            quantity: 6,
          },
          position: 1,
          selectedOptions: [
            {
              variationId: "variation-flavor",
              optionId: "option-strawberry",
            },
          ],
          existImage: {
            path: "products/variants/strawberry-2.png",
            alt: "Strawberry front alt",
          },
          newImage: undefined,
        },
      ],
    });

    expect(result.success).toBe(false);
  });
});
