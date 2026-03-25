import { z } from "zod";

export const warehouseDtoSchema = z.object({
  id: z.cuid(),
  name: z.string(),
});

export const warehouseListDtoSchema = z.array(warehouseDtoSchema);

export type WarehouseDto = z.infer<typeof warehouseDtoSchema>;
