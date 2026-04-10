import { getProductListForProductCard } from "@/lib/products/repositories/queries/get-product-list-for-product-card.repository";
import ProductCard from "./product-card";

const ProductGallery = async () => {
  const products = await getProductListForProductCard();
  return (
    <div className="mx-auto grid w-full max-w-screen-2xl grid-cols-[repeat(auto-fit,minmax(16rem,16rem))] justify-center gap-4 px-4">
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}
    </div>
  );
};

export default ProductGallery;
