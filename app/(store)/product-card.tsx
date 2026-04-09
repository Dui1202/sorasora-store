import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Prisma } from "../generated/prisma/browser";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";

type ProductForProductCard = Prisma.ProductGetPayload<{
  select: {
    name: true;
    sellMode: true;
    preorderReleaseAt: true;
    variants: {
      select: {
        sellingPrice: true;
      };
    };
    productImages: {
      select: {
        path: true;
      };
    };
  };
}>;
interface Props {
  product: ProductForProductCard;
}

/**
 * Renders a compact storefront card using the minimal product list payload.
 */
const ProductCard = ({ product }: Props) => {
  const primaryImage = product.productImages[0]?.path;
  const publicCoverImageURL = getProductImagePublicUrl(primaryImage);
  const primaryVariant = product.variants[0];

  return (
    <Card className="relative w-64 pt-0">
      {/* Fallback to an empty image block when the product has no images yet. */}
      {primaryImage ? (
        <img
          src={publicCoverImageURL}
          className="relative z-20 aspect-square object-cover w-full rounded-lg"
          alt={product.name}
        />
      ) : (
        <div className="rounded-2xl bg-muted aspect-square" />
      )}
      <CardHeader className="p-2">
        <p>{product.sellMode === "PREORDER" ? "Preorder" : "In Stock"}</p>
        <CardTitle className="truncate">{product.name}</CardTitle>
      </CardHeader>
      <CardContent className="p-2">
        {/* Price falls back gracefully when an active variant is missing. */}
        <p>{primaryVariant ? primaryVariant.sellingPrice : "Unavailable"}</p>
      </CardContent>
    </Card>
  );
};

export default ProductCard;
