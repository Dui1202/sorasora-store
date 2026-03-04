import { z } from "zod";

export const franchiseDtoSchema = z.object({
  id: z.cuid(),
  name: z.string(),
});

export const franchiseListDtoSchema = z.array(franchiseDtoSchema);

export type FranchiseDto = z.infer<typeof franchiseDtoSchema>;
