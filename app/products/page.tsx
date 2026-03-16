import axios from "axios";
import React from "react";
import type { Prisma } from "@/app/generated/prisma/client";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { getProductImagePublicUrl } from "@/lib/infrastructure/storage/get-product-image-public-url";

const ProductsPage = async () => {
  const tableColumns = [
    { id: "index", label: "Index" },
    { id: "products", label: "Product(s)" },
    { id: "costPrice", label: "Cost Price" },
    { id: "sellingPrice", label: "Selling Price" },
    { id: "action", label: "Action" },
  ];
  const products = await fetchProducts();
  return (
    <Table>
      <TableHeader>
        <TableRow>
          {tableColumns.map((column) => {
            return <TableHead key={column.id}>{column.label}</TableHead>;
          })}
        </TableRow>
      </TableHeader>
      <TableBody>
        {products.map(async (product, index) => {
          const coverProductImagePublicUrl = await getProductImagePublicUrl(
            product.productImages[0].path,
          );
          return (
            <TableRow>
              <TableCell>{index + 1}</TableCell>
              <TableCell>
                <div>
                  <img src={coverProductImagePublicUrl} />
                </div>
              </TableCell>
            </TableRow>
          );
        })}
      </TableBody>
    </Table>
  );
};

async function fetchProducts() {
  const products = await axios.get<ProductWithRelations[]>("/api/products");
  return products.data;
}

export default ProductsPage;
