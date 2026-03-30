"use client";

import { SkeletonTable } from "@/components/skeleton-table";
import ProductTable from "@/feature/products/product-table/product-table";
import { useProducts } from "@/hooks/use-products";
import axios from "axios";

const ProductsPage = () => {
  const {
    data: products,
    error,
    isLoading: isLoadingProducts,
  } = useProducts();

  if (isLoadingProducts) {
    return <SkeletonTable />;
  }

  if (error) {
    const errorMessage = axios.isAxiosError<{ message?: string }>(error)
      ? error.response?.data?.message ?? "Failed to load products."
      : "Failed to load products.";

    return (
      <p className="p-5 text-sm text-destructive">{errorMessage}</p>
    );
  }

  return (
    <div className="p-5">
      <ProductTable products={products ?? []} />
    </div>
  );
};

export default ProductsPage;
