import { Card } from "@/components/ui/card";
import {
  Carousel,
  CarouselItem,
  CarouselNext,
  CarouselPrevious,
} from "@/components/ui/carousel";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { getProductWithRelationsByIdService } from "@/lib/products";
import { notFound } from "next/navigation";
import React from "react";

const ProductDetailPage = async () => {
  const imagesPublicURL = "https://dummyjson.com/image/150";
  const imageProduct = [1, 2, 3, 4, 5];
  return (
    <Card>
      <div className="grid grid-cols-2">
        <div className="flex flex-col">
          <img src={imagesPublicURL} className="aspect-square" />
          <Carousel>
            {imageProduct.map((item) => (
              <CarouselItem key={item}>
                <img src={imagesPublicURL} className="aspect-square" />
              </CarouselItem>
            ))}
            <CarouselPrevious />
            <CarouselNext />
          </Carousel>
        </div>
      </div>
    </Card>
  );
};

export default ProductDetailPage;
