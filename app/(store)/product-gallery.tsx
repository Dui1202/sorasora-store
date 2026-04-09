import { getProductListForProductCard } from "@/lib/products/repositories/queries/get-product-list-for-product-card.repository";
import ProductCard from "./product-card";

const ProductGallery = async () => {
  const products = await getProductListForProductCard();
  return (
    <div className="grid-cols-6 gap-2 grid mx-10 max-w-m">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
};

export default ProductGallery;
