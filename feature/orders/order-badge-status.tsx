import { OrderStatus } from "./order-status";
import { Badge } from "@/components/ui/badge";
import { cva } from "class-variance-authority";
import React from "react";

interface Props {
  status: OrderStatus;
}
type OrderStatusColor = "red" | "blue" | "green" | "yellow";

const OrderBadgeStatus = ({ status }: Props) => {
  const badgeVariants = cva(
    "inline-flex items-center rounded-md px-2 py-1 text-sm",
    {
      variants: {
        color: {
          red: "bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-300",
          blue: "bg-blue-50 text-blue-700 dark:bg-blue-950 dark:text-blue-300",
          green:
            "bg-green-50 text-green-700 dark:bg-green-950 dark:text-green-300",
          yellow:
            "bg-yellow-50 text-yellow-700 dark:bg-yellow-950 dark:text-yellow-300",
        },
      },
      defaultVariants: {
        color: "blue",
      },
    },
  );

  const statusMap: Record<
    OrderStatus,
    { label: string; color: OrderStatusColor }
  > = {
    [OrderStatus.PENDING]: { label: "Pending", color: "yellow" },
    [OrderStatus.CONFIRMED]: { label: "Confirmed", color: "blue" },
    [OrderStatus.COMPLETED]: { label: "Completed", color: "green" },
    [OrderStatus.CANCELLED]: { label: "Cancelled", color: "red" },
  };

  return (
    <Badge className={badgeVariants({ color: statusMap[status].color })}>
      {statusMap[status].label}
    </Badge>
  );
};

export default OrderBadgeStatus;
