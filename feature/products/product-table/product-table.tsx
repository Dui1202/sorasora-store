import { SkeletonTable } from "@/components/skeleton-table";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useProducts } from "@/hooks/use-products";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/browser/get-product-image-public-url";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import ProductRow from "./product-row";

type ProductTableProps = {
  products: ProductWithRelations[];
};

const ProductTable = ({ products }: ProductTableProps) => {
  const tableColumns = [
    { id: "index", label: "Index" },
    { id: "products", label: "Product(s)" },
    { id: "costPrice", label: "Cost Price" },
    { id: "sellingPrice", label: "Selling Price" },
    { id: "action", label: "Action" },
  ];

  return (
    <Table>
      <TableHeader color="">
        <TableRow>
          {tableColumns.map((column) => {
            return <TableHead key={column.id}>{column.label}</TableHead>;
          })}
        </TableRow>
      </TableHeader>
      <TableBody>
        {products?.map((product, index) => {
          if (product.variants.length === 1) {
            return (
              <ProductRow key={product.id} product={product} index={index} />
            );
          }
        })}
      </TableBody>
    </Table>
  );
};

export default ProductTable;
