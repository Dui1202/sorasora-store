"use client";

import { Prisma } from "@/app/generated/prisma/browser";
import { useQuery } from "@tanstack/react-query";
import axios from "axios";

type ProductWithRelations = Prisma.ProductGetPayload<{
  include: {
    productImages: true;
    variants: {
      include: {
        image: true;
      };
    };
  };
}>;

export const useProducts = () =>
  useQuery<ProductWithRelations[]>({
    queryKey: ["users"],
    queryFn: () =>
      axios
        .get<ProductWithRelations[]>("/api/products")
        .then((res) => res.data),
    staleTime: 5 * 1000,
    retry: 3,
  });
