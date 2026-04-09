import { Prisma } from "@/app/generated/prisma/client";

export type CartOwnerContext =
  | { userId: string; guestCartToken?: undefined }
  | { userId?: undefined; guestCartToken: string }
  | null;

export type ProductVariantForCartItem = Prisma.ProductVariantGetPayload<{
  include: {
    image: true;
    product: {
      select: {
        id: true;
        name: true;
        sellMode: true;
        preorderReleaseAt: true;
      };
    };
  };
}>;
