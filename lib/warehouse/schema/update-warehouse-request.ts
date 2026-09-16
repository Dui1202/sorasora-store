import { z } from "zod";
import { warehouseBaseSchema } from "./warehouse-base";

export const updateWarehouseRequestSchema = warehouseBaseSchema.omit({
  isArchived: true,
});

export type UpdateWarehouseRequest = z.infer<
  typeof updateWarehouseRequestSchema
>;
