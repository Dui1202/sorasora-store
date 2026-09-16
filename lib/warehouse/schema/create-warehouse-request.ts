import z from "zod";
import { warehouseBaseSchema } from "./warehouse-base";

export const createWarehouseRequestSchema = warehouseBaseSchema.omit({
  id: true,
  isArchived: true,
});

export type CreateWarehouseRequest = z.infer<
  typeof createWarehouseRequestSchema
>;
