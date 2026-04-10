import { Button } from "@/components/ui/button";
import { ShoppingCart } from "lucide-react";

interface Props {
  className?: string;
}

const ProductActionButtons = ({ className }: Props) => {
  return (
    <div className={`flex flex-row gap-3 w-3/5 h-15 ${className}`}>
      <Button
        variant={"secondary"}
        className="basis-3/5 text-2xl h-full flex gap-4"
        size="lg"
      >
        <ShoppingCart className="size-5" />
        <p>Add to cart</p>
      </Button>
      <Button className="basis-2/5 h-full text-2xl">Buy now</Button>
    </div>
  );
};

export default ProductActionButtons;
