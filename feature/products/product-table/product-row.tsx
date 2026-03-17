import { TypographyH2, TypographyMuted } from "@/components/typography";
import { TableCell, TableRow } from "@/components/ui/table";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import React from "react";

type ProductRowProps = {
  product: ProductWithRelations;
  index: number;
};
const ProductRow = ({ product, index }: ProductRowProps) => {
  const coverProductImagePublicUrl = getProductImagePublicUrl(
    product.productImages[0].path,
  );
  return (
    <TableRow key={product.id}>
      <TableCell>{index + 1}</TableCell>
      <TableCell>
        <div className="flex gap-4">
          <img src={coverProductImagePublicUrl} width="100" height="100" />
          <div>
            <TypographyH2>{product.name}</TypographyH2>
            <TypographyMuted>{product.id}</TypographyMuted>
          </div>
        </div>
      </TableCell>
      <TableCell>{product.variants[0].costPrice}</TableCell>
      <TableCell>{product.variants[0].sellingPrice}</TableCell>
    </TableRow>
  );
};

export default ProductRow;
