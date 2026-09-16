export const warehouseKeys = {
  all: ["warehouses"] as const,
  detail: (id: string) => ["warehouses", id] as const,
};
