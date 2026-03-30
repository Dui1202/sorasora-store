import { Card } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
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
    <Card className="p-5">
      <Table>
        <TableHeader>
          <TableRow>
            {tableColumns.map((column) => {
              return <TableHead key={column.id}>{column.label}</TableHead>;
            })}
          </TableRow>
        </TableHeader>
        <TableBody>
          {products.length === 0 ? (
            <TableRow>
              <TableCell
                colSpan={5}
                className="py-6 text-center text-muted-foreground"
              >
                No products found.
              </TableCell>
            </TableRow>
          ) : null}
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
