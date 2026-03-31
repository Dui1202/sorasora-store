import ProductTable from "@/feature/products/product-table/product-table";
import { listProductsService } from "@/lib/products";

const ProductsPage = async () => {
  const products = await listProductsService();

  return (
    <div className="p-5">
      <ProductTable products={products ?? []} />
    </div>
  );
};

export default ProductsPage;
