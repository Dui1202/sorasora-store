export const SupplierCountry = {
  JAPAN: "JAPAN",
  CHINA: "CHINA",
} as const;

export type SupplierCountry =
  (typeof SupplierCountry)[keyof typeof SupplierCountry];
