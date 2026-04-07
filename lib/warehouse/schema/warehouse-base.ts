import z from "zod";

export const warehouseBaseSchema = z.object({
  name: z
    .string()
    .min(1, "Warehouse name is required!")
    .max(255, "Warehouse name must be less than 255 characters!"),
});

export type Warehouse = z.infer<typeof warehouseBaseSchema>;
