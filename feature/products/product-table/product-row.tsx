import { TypographyH2, TypographyMuted } from "@/components/typography";
import { TypographyH3 } from "@/components/typography/typographyh3";
import { Button } from "@/components/ui/button";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { TableCell, TableRow } from "@/components/ui/table";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { ChevronUp, Table } from "lucide-react";
import React, { Fragment, useState } from "react";
import ProductActionButtons from "./product-action-buttons";

type ProductRowProps = {
  product: ProductWithRelations;
  index: number;
};
const ProductRow = ({ product, index }: ProductRowProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const coverProductImagePublicUrl = getProductImagePublicUrl(
    product.productImages[0].path,
  );

  const hasVariants = product.variants.length > 1;

  const onShowVariantsClick = () => setIsOpen(!isOpen);
  return (
    <>
      <TableRow key={product.id} className={`relative `}>
        <TableCell className={`align-top ${hasVariants && "pb-10 "}`}>
          {index + 1}
        </TableCell>
        <TableCell className={`align-top ${hasVariants && "pb-10 "}`}>
          <div className="flex gap-4">
            <img src={coverProductImagePublicUrl} width="100" height="100" />
            <div>
              <TypographyH3>{product.name}</TypographyH3>
              <TypographyMuted>Product Id: {product.id}</TypographyMuted>
            </div>
          </div>
          {hasVariants && (
            <div className="absolute inset-x-0 bottom-2 justify-center flex  ">
              <Button onClick={onShowVariantsClick} variant="ghost">
                Show Variants <ChevronUp />
              </Button>
            </div>
          )}
        </TableCell>
        <TableCell className={`align-top ${hasVariants && "pb-10 "}`}>
          {product.variants[0].costPrice}
        </TableCell>
        <TableCell className={`align-top ${hasVariants && "pb-10 "}`}>
          {product.variants[0].sellingPrice}
        </TableCell>
        <TableCell className={`align-top ${hasVariants && "pb-10 "}`}>
          <ProductActionButtons id={product.id} />
        </TableCell>
      </TableRow>

      {isOpen &&
        hasVariants &&
        product.variants.map((variant) => {
          const variantImageUrl = getProductImagePublicUrl(variant.image!.path);
          return (
            <TableRow key={variant.id}>
              <TableCell></TableCell>
              <TableCell className="pl-10">
                <div className="flex gap-4">
                  <img src={variantImageUrl} width="100" height="100" />
                  <div>
                    <TypographyH3>{variant.name}</TypographyH3>
                    <TypographyMuted>Variant Id: {variant.id}</TypographyMuted>
                    <TypographyMuted>
                      Variant SKU: {variant.sku}
                    </TypographyMuted>
                  </div>
                </div>
              </TableCell>
              <TableCell className="align-top">{variant.costPrice}</TableCell>
              <TableCell className="align-top">
                {variant.sellingPrice}
              </TableCell>
            </TableRow>
          );
        })}
    </>
  );
};

export default ProductRow;
