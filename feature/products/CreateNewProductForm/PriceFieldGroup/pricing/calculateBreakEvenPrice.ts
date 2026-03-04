import { SupplierCountry } from "@/app/generated/prisma/enums";
import {
  COUNTRY_CONFIG,
  GLOBAL_COSTS,
  PLATFORM_CONFIG,
  PlatformCode,
} from "./config";

export const calculateBreakEvenPrice = (
  supplierCountry: SupplierCountry,
  platformCode: PlatformCode,
  orginalPrice: number,
) => {
  if (!supplierCountry) {
    return;
  }

  if (!orginalPrice) {
    return;
  }
  const landedPrice =
    orginalPrice *
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

  return breakEvenPrice;
};
