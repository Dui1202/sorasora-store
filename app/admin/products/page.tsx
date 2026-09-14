import UnavailableNotice from "@/components/unavailable-notice";
import ProductTable from "@/feature/products/product-table/product-table";

export default function ProductsPage() {
  return (
    <div className="space-y-4 p-5">
      <UnavailableNotice feature="Product management" />
      <ProductTable products={[]} />
    </div>
  );
}
