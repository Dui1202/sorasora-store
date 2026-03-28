"use client";

import { SkeletonTable } from "@/components/skeleton-table";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import ProductTable from "@/feature/products/product-table/product-table";
import { useProducts } from "@/hooks/use-products";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { Skeleton } from "@radix-ui/themes";

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
