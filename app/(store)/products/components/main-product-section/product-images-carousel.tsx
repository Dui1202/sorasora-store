import {
  Carousel,
  CarouselContent,
  CarouselItem,
  CarouselNext,
  CarouselPrevious,
} from "@/components/ui/carousel";

interface Props {
  imagePublicURLs: string[];
}

const ProductImagesCarousel = ({ imagePublicURLs }: Props) => {
  return (
    <Carousel className="w-9/12 max-w-2xl px-10 sm:px-12 md:px-0">
      <CarouselContent>
        {imagePublicURLs.map((item, index) => (
          <CarouselItem
            key={item + index}
            className="basis-1/3 sm:basis-1/4 lg:basis-1/5"
          >
            <img
              src={item}
              alt={`Product thumbnail ${item}`}
              className="aspect-square w-full rounded-2xl object-cover"
            />
          </CarouselItem>
        ))}
      </CarouselContent>

      <CarouselPrevious className="left-0 sm:left-1 md:-left-12" />
      <CarouselNext className="right-0 sm:right-1 md:-right-12" />
    </Carousel>
  );
};

export default ProductImagesCarousel;
