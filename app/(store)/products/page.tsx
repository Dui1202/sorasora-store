import ProductCard from "../product-card";
import { BreadcrumbSection } from "./components/breadcrumb-section";
import MainProductSection from "./components/main-product-section/main-product-section";
import ProductDescriptionSection from "./components/product-description-section";

const ProductDetailPage = async () => {
  return (
    <div className="flex flex-col gap-3 mx-10 my-2">
      <BreadcrumbSection />
      <MainProductSection />
      <ProductDescriptionSection />
    </div>
  );
};

export default ProductDetailPage;
