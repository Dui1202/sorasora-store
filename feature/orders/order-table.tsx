"use client";

import { OrderStatus } from "@/app/generated/prisma/enums";
import { Card } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import OrderBadgeStatus from "./order-badge-status";

const OrderTable = () => {
  const orders = [
    {
      id: "OD-000001",
      customerName: "John Doe",
      shippingAddress: "123 Main St",
      status: OrderStatus.PENDING,
      totalAmount: "$100.00",
      orderDate: "2022-01-01 12:00:00",
    },
    {
      id: "OD-000002",
      customerName: "Jane Smith",
      shippingAddress: "456 Elm St",
      status: OrderStatus.CONFIRMED,
      totalAmount: "$50.00",
      orderDate: "2022-01-02 13:30:00",
    },
  ];

  return (
    <Card className="p-5">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Order ID</TableHead>
            <TableHead>Customer Name</TableHead>
            <TableHead>Shipping Address</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Total Amount</TableHead>
            <TableHead>Order Date</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {orders.map((order) => (
            <TableRow key={order.id}>
              <TableCell>{order.id}</TableCell>
              <TableCell>{order.customerName}</TableCell>
              <TableCell>{order.shippingAddress}</TableCell>
              <TableCell>
                <OrderBadgeStatus status={order.status} />
              </TableCell>
              <TableCell>{order.totalAmount}</TableCell>
              <TableCell>{order.orderDate}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Card>
  );
};

export default OrderTable;
