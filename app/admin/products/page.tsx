"use client";

import { SkeletonTable } from "@/components/skeleton-table";
import ProductTable from "@/feature/products/product-table/product-table";
import { useProducts } from "@/hooks/use-products";

const ProductsPage = () => {
  const { data: products, isLoading: isLoadingProducts } = useProducts();
  if (isLoadingProducts) {
    return <SkeletonTable />;
  }
  return (
    <div className="p-5">
      <ProductTable products={products ?? []} />
    </div>
  );
};

export default ProductsPage;
