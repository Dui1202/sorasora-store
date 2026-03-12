import React from "react";
import { Variation } from "../VariationFieldGroup/type";
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

type VariationPriceTableFieldProps = {
  variations: Variation[];
};
const VariationPriceTableField = ({
  variations,
}: VariationPriceTableFieldProps) => {
  if (variations.length === 0) {
    return null;
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Variation 1</TableHead>
          <TableHead>Price</TableHead>
          <TableHead></TableHead>
          <TableHead></TableHead>
        </TableRow>
      </TableHeader>
      <TableBody></TableBody>
    </Table>
  );
};

export default VariationPriceTableField;
