import { Badge } from "@/components/ui/badge";
import { TableCell, TableRow } from "@/components/ui/table";

const OrderRow = () => {
  return (
    <TableRow>
      <TableCell>OD-000001</TableCell>
      <TableCell>John Doe</TableCell>
      <TableCell>123 Main St</TableCell>
      <TableCell>
        <Badge>Success</Badge>
      </TableCell>
      <TableCell>$100.00</TableCell>
      <TableCell>2022-01-01 12:00:00</TableCell>
    </TableRow>
  );
};

export default OrderRow;
