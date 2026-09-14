import UnavailableNotice from "@/components/unavailable-notice";
import ProductForm from "@/feature/products/product-form/product-form";

export default function CreateNewProductPage() {
  return (
    <>
      <div className="px-5 pt-5">
        <UnavailableNotice feature="Product creation" />
      </div>
      <ProductForm mode="create" />
    </>
  );
}
