import { TypographyH1, TypographyH2 } from "@/components/typography";
import { TypographyH3 } from "@/components/typography/typographyh3";
import { Card } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import ProductActionButtons from "./product-action-buttons";
import ProductImagesCarousel from "./product-images-carousel";
import QuantityInput from "./quantity-input";
const MainProductSection = () => {
  const imagesPublicURL = "https://dummyjson.com/image/150";
  const imageProduct = [1, 2, 3, 4, 5, 6, 7, 8];
  const imagePublicURLs = imageProduct.map(
    (_item) => `https://dummyjson.com/image/150`,
  );
  const productPlaceholder = {
    name: "Product Name",
    price: 100000,
    originalPrice: 200000,
    description: "Product description",
    status: "In Stock",
    brand: "Apex",
  };
  return (
    <Card className="overflow-hidden">
      <div className="grid grid-cols-1 gap-4 p-4 sm:gap-6 sm:p-6 lg:grid-cols-[minmax(0,42rem)_minmax(0,1fr)] lg:gap-10">
        <div className="flex min-w-0 flex-col items-center gap-3">
          <img
            src={imagesPublicURL}
            alt="Product preview"
            className="aspect-square w-full max-w-xl rounded-2xl object-cover"
          />
          <ProductImagesCarousel imagePublicURLs={imagePublicURLs} />
        </div>
        <div className="flex flex-col gap-5">
          <TypographyH1>{productPlaceholder.name}</TypographyH1>
          <div className="flex flex-row gap-2">
            <TypographyH3>Status: {productPlaceholder.status}</TypographyH3>
            <Separator orientation="vertical" />
            <TypographyH3>Brand: {productPlaceholder.brand}</TypographyH3>
          </div>
          <Card className="flex flex-row px-10">
            <TypographyH2>{productPlaceholder.price} VND</TypographyH2>
            <TypographyH3 className="line-through self-end">
              {productPlaceholder.originalPrice} VND
            </TypographyH3>
          </Card>
          <div className="flex gap-3">
            <TypographyH3>Quantity:</TypographyH3>
            <QuantityInput />
          </div>
          <ProductActionButtons />
        </div>
      </div>
    </Card>
  );
};

export default MainProductSection;
