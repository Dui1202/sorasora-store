import { SupplierCountry } from "@/lib/domain/products/supplier-country";

export type PlatformCode = "SHOPEE" | "NONE";

export const COUNTRY_CONFIG = {
  CHINA: {
    currencyCode: "CNY",
    exchangeRateToVND: 4000,
    handlingPercent: 0.03,
  },
  JAPAN: {
    currencyCode: "JPY",
    exchangeRateToVND: 170,
    handlingPercent: 0.05,
  },
} satisfies Record<
  SupplierCountry,
  {
    currencyCode: string;
    exchangeRateToVND: number;
    handlingPercent: number;
  }
>;

export const PLATFORM_CONFIG = {
  SHOPEE: {
    percentFee: 0.2541,
    fixedFee: 3000,
  },
  NONE: {
    percentFee: 0,
    fixedFee: 0,
  },
} satisfies Record<
  PlatformCode,
  {
    percentFee: number;
    fixedFee: number;
  }
>;

export const GLOBAL_COSTS = {
  returnHandlingPercent: 0.02,
  taxPercent: 0.015,
  marketingPercent: 0,
  packagingFlat: 2000,
};
