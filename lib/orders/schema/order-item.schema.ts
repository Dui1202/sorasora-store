import { Prisma, PreorderStage, SellMode } from "@/app/generated/prisma/browser";
import { z } from "zod";

export const sellModeSnapshotSchema = z.enum(SellMode);
export const preorderStageSchema = z.enum(PreorderStage);

const nonNegativeInt = (fieldName: string) =>
  z.number().int().min(0, `${fieldName} must be 0 or greater`);

const decimalLikeSchema = z.union([
  z.instanceof(Prisma.Decimal),
  z.number().min(0, "Tax rate snapshot must be 0 or greater"),
  z
    .string()
    .trim()
    .min(1, "Tax rate snapshot is required")
    .refine((value) => {
      const parsedValue = Number(value);

      return Number.isFinite(parsedValue) && parsedValue >= 0;
    }, "Tax rate snapshot must be a valid decimal value"),
]);

/**
 * Snapshot fields persisted on the order item at checkout time.
 *
 * These values preserve the product, pricing, and fulfillment context that the
 * order item was created from so downstream workflows do not rely on mutable
 * catalog data.
 */
export const orderItemSnapshotSchema = z.object({
  productNameSnapshot: z.string().trim().min(1, "Product name snapshot is required"),
  variantNameSnapshot: z.string().trim().optional(),
  skuSnapshot: z.string().trim().optional(),
  imageUrlSnapshot: z.string().trim().optional(),
  sellModeSnapshot: sellModeSnapshotSchema,
  releaseDateSnapshot: z.date().optional(),
  preorderStage: preorderStageSchema.default(PreorderStage.NONE),
  requiresShipping: z.boolean().default(true),
  weightGramsSnapshot: nonNegativeInt("Weight grams snapshot").optional(),
  taxRateSnapshot: decimalLikeSchema.nullish(),
});

/**
 * Quantity and monetary fields owned by the order item workflow.
 */
export const orderItemStateSchema = z.object({
  unitPriceAmount: nonNegativeInt("Unit price amount"),
  quantityOrdered: z.number().int().min(1, "Quantity ordered must be at least 1"),
  quantityCancelled: nonNegativeInt("Quantity cancelled").default(0),
  quantityFulfilled: nonNegativeInt("Quantity fulfilled").default(0),
  quantityRefunded: nonNegativeInt("Quantity refunded").default(0),
  lineSubtotalAmount: nonNegativeInt("Line subtotal amount"),
  lineDiscountAmount: nonNegativeInt("Line discount amount").default(0),
  lineTotalAmount: nonNegativeInt("Line total amount"),
});

/**
 * Base write input owned by order creation and persistence workflows.
 */
export const orderItemBaseSchema = z.object({
  orderId: z.cuid("Invalid order id"),
  productId: z.cuid("Invalid product id").optional(),
  variantId: z.cuid("Invalid variant id").optional(),
});

/**
 * Full order item contract including workflow identifiers and checkout
 * snapshots.
 */
export const orderItemSchema = orderItemBaseSchema
  .extend({
    ...orderItemSnapshotSchema.shape,
    ...orderItemStateSchema.shape,
  })
  .superRefine((orderItem, context) => {
    if (orderItem.quantityCancelled > orderItem.quantityOrdered) {
      context.addIssue({
        code: "custom",
        message: "Quantity cancelled cannot exceed quantity ordered",
        path: ["quantityCancelled"],
      });
    }

    if (orderItem.quantityFulfilled > orderItem.quantityOrdered) {
      context.addIssue({
        code: "custom",
        message: "Quantity fulfilled cannot exceed quantity ordered",
        path: ["quantityFulfilled"],
      });
    }

    if (orderItem.quantityRefunded > orderItem.quantityOrdered) {
      context.addIssue({
        code: "custom",
        message: "Quantity refunded cannot exceed quantity ordered",
        path: ["quantityRefunded"],
      });
    }

    if (
      orderItem.quantityCancelled + orderItem.quantityFulfilled >
      orderItem.quantityOrdered
    ) {
      context.addIssue({
        code: "custom",
        message: "Cancelled and fulfilled quantities cannot exceed quantity ordered",
        path: ["quantityFulfilled"],
      });
    }
  });

export type OrderItemSnapshot = z.infer<typeof orderItemSnapshotSchema>;
export type OrderItemState = z.infer<typeof orderItemStateSchema>;
export type OrderItemInput = z.infer<typeof orderItemBaseSchema>;
export type OrderItem = z.infer<typeof orderItemSchema>;
