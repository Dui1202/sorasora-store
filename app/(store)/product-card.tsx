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
    <Card className="relative w-64 gap-4 pt-3 pb-6 px-3">
      {primaryImage ? (
        <img
          src={publicCoverImageURL}
          className="aspect-square w-full rounded-lg object-cover"
          alt={product.name}
        />
      ) : (
        <div className="aspect-square w-full rounded-2xl bg-muted" />
      )}

      <CardHeader className="p-0">
        <p>{product.sellMode === "PREORDER" ? "Preorder" : "In Stock"}</p>
        <CardTitle className="truncate">{product.name}</CardTitle>
      </CardHeader>

      <CardContent className="p-0">
        <p>{primaryVariant ? primaryVariant.sellingPrice : "Unavailable"}</p>
      </CardContent>
    </Card>
  );
};

export default ProductCard;
