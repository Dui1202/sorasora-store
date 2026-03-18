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
import { Card } from "@/components/ui/card";

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
    <Card className="p-5">
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
            return (
              <ProductRow key={product.id} product={product} index={index} />
            );
          })}
        </TableBody>
      </Table>
    </Card>
  );
};

export default ProductTable;
