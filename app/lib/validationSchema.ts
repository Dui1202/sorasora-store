import { number, string, z } from "@/node_modules/zod";

export const createProductSchema = z.object({
  name: string()
    .min(1, "Title is required!")
    .max(255, "Title is too long! Must be less than 255 characters"),
  description: string().min(1, "Description is required!").max(65535),
  price: number().nonnegative("Price can not be negative!"),
  sku: string()
    .max(255, "Title is too long! Must be less than 255 characters")
    .optional(),
  quantity: number().nonnegative("Quantity can not be negative!").optional(),
});
