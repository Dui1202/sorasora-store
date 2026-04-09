import { z } from "zod";

/**
 * Base shipping information input used by create and update workflows.
 *
 * This shape captures the address data owned by the shipping-info feature
 * without persistence-managed identifiers or timestamps.
 */
export const shippingInfoBaseSchema = z.object({
  recipientName: z
    .string()
    .trim()
    .min(1, "Recipient name is required"),
  phone: z
    .string()
    .trim()
    .min(1, "Phone number is required"),
  addressLine: z
    .string()
    .trim()
    .min(1, "Address line is required"),
  district: z
    .string()
    .trim()
    .min(1, "District is required"),
  province: z
    .string()
    .trim()
    .min(1, "Province is required"),
  isDefault: z.boolean().default(false),
});

/**
 * Shipping information create input.
 *
 * The user relationship is part of the application boundary for new records.
 */
export const createShippingInfoSchema = shippingInfoBaseSchema.extend({
  userId: z.cuid("Invalid user id"),
});

/**
 * Shipping information update input.
 *
 * Partial updates are allowed for mutable address fields.
 */
export const updateShippingInfoSchema = shippingInfoBaseSchema.partial();

/**
 * Full persisted shipping information contract.
 */
export const shippingInfoSchema = createShippingInfoSchema.extend({
  id: z.cuid("Invalid shipping info id"),
  createdAt: z.date(),
  updatedAt: z.date(),
});

export type ShippingInfoInput = z.infer<typeof shippingInfoBaseSchema>;
export type CreateShippingInfoInput = z.infer<typeof createShippingInfoSchema>;
export type UpdateShippingInfoInput = z.infer<typeof updateShippingInfoSchema>;
export type ShippingInfo = z.infer<typeof shippingInfoSchema>;
