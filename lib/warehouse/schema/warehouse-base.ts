import z from "zod";

export const warehouseBaseSchema = z.object({
  id: z.string(),
  name: z
    .string()
    .min(1, "Warehouse name is required!")
    .max(255, "Warehouse name must be less than 255 characters!"),
  isDefault: z.boolean(),
  isArchived: z.boolean(),
});

export type Warehouse = z.infer<typeof warehouseBaseSchema>;
