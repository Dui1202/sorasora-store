import { z } from "zod";

export const createProductFormDataSchema = z.object({});

export type CreateProductFormData = z.infer<typeof createProductFormDataSchema>;
