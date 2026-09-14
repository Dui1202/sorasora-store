import { SupplierCountry } from "@/lib/domain/products/supplier-country";
import {
  COUNTRY_CONFIG,
  GLOBAL_COSTS,
  PLATFORM_CONFIG,
  PlatformCode,
} from "./config";

export const calculateBreakEvenPrice = (
  supplierCountry: SupplierCountry,
  platformCode: PlatformCode,
  costPrice: number,
) => {
  if (!supplierCountry) {
    return;
  }

  if (!costPrice) {
    return;
  }
  const landedPrice =
    costPrice *
    COUNTRY_CONFIG[supplierCountry].exchangeRateToVND *
    (1 + COUNTRY_CONFIG[supplierCountry].handlingPercent);

  const totalPercentageFee =
    GLOBAL_COSTS.marketingPercent +
    GLOBAL_COSTS.returnHandlingPercent +
    GLOBAL_COSTS.taxPercent +
    PLATFORM_CONFIG[platformCode].percentFee;

  const totalFixedFee =
    GLOBAL_COSTS.packagingFlat + PLATFORM_CONFIG[platformCode].fixedFee;

  const breakEvenPrice =
    (landedPrice + totalFixedFee) / (1 - totalPercentageFee);

  return Math.ceil(breakEvenPrice);
};

export const getFormattedPrice = (price: number) => {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(price);
};
