import { Button } from "@/components/ui/button";
import type { ProductListItem } from "./types";

type Props = {
  product: ProductListItem;
};

export default function ProductActionButtons({ product }: Props) {
  return (
    <div className="flex flex-col gap-2">
      <Button variant="secondary" disabled>Edit</Button>
      <Button variant="secondary" disabled>
        {product.isPublished ? "Delist" : "Publish"}
      </Button>
      <Button variant="destructive" disabled>Delete</Button>
    </div>
  );
}
