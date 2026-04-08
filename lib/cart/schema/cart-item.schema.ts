import { SellMode } from "@/app/generated/prisma/enums";
import { z } from "zod";

/**
 * Sell mode snapshot stored on the cart item.
 *
 * This keeps the cart aligned with the product state captured when the item
 * was added or refreshed.
 */
export const sellModePreviewSchema = z.enum(SellMode);

/**
 * Snapshot fields persisted on the cart item.
 *
 * These values are denormalized from product and variant data so cart reads can
 * render stable previews without depending entirely on live product queries.
 */
export const cartItemPreviewSchema = z.object({
  productNamePreview: z.string().trim().optional(),
  variantNamePreview: z.string().trim().optional(),
  skuPreview: z.string().trim().optional(),
  imagePathPreview: z.string().trim().optional(),
  unitPricePreviewAmount: z
    .number()
    .int()
    .min(0, "Unit price preview amount must be 0 or greater")
    .optional(),
  lineTotalPreviewAmount: z
    .number()
    .int()
    .min(0, "Line total preview amount must be 0 or greater")
    .optional(),
  sellModePreview: sellModePreviewSchema.optional(),
  preorderReleaseDatePreview: z.date().optional(),
  preorderEtaNotePreview: z.string().trim().optional(),
});

/**
 * Base cart item input owned by the cart workflow.
 *
 * This shape identifies the cart, product, selected variant, and quantity
 * before preview snapshot fields are attached.
 */
export const cartItemBaseSchema = z.object({
  cartId: z.cuid("Invalid cart id"),
  productId: z.cuid("Invalid product id"),
  variantId: z.cuid("Invalid variant id"),
  quantity: z.number().int().min(1, "Quantity must be at least 1"),
  isSelected: z.boolean().default(true),
});

/**
 * Full cart item contract.
 *
 * Combines workflow-owned identifiers and quantity state with the preview
 * snapshot fields stored for cart rendering.
 */
export const cartItemSchema = cartItemBaseSchema.extend({
  ...cartItemPreviewSchema.shape,
});

/**
 * Preview-only cart item snapshot type.
 */
export type CartItemPreview = z.infer<typeof cartItemPreviewSchema>;
/**
 * Base cart item write input type.
 */
export type CartItemInput = z.infer<typeof cartItemBaseSchema>;
/**
 * Complete cart item type including identifiers, quantity, and preview fields.
 */
export type CartItem = z.infer<typeof cartItemSchema>;
