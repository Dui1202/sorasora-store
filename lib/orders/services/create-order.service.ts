import { randomUUID } from "node:crypto";

import { AppError } from "@/core/errors/errors";
import prisma from "@/lib/infrastructure/database/prisma";
import {
  cartItemSchema,
  type CartItem,
} from "@/lib/cart/schema/cart-item.schema";
import { getCartService } from "@/lib/cart/service/get-cart.service";
import type { CartOwnerContext } from "@/lib/cart/type";
import { cartItemToOrderItemMapper } from "../mapper/cart-item-to-order-item.mapper";

export type CreateOrderServiceInput = {
  owner: Exclude<CartOwnerContext, null>;
  customerName: string;
  customerPhone: string;
  customerEmail?: string;
  note?: string;
};

export async function createOrderService(input: CreateOrderServiceInput) {
  const cart = await getCartService(input.owner);

  if (!cart) {
    throw new AppError("CART_NOT_FOUND", "Cart not found", 404);
  }

  const selectedCartItems = cart.items
    .map((item) => cartItemSchema.parse(item))
    .filter((item) => item.isSelected);

  if (selectedCartItems.length === 0) {
    throw new AppError(
      "INVALID_REQUEST_BODY",
      "No selected cart items found for checkout.",
      400,
    );
  }

  const subtotalAmount = selectedCartItems.reduce(
    (total, item) => total + getCartItemLineTotalAmount(item),
    0,
  );

  const discountAmount = 0;
  const shippingAmount = 0;
  const taxAmount = 0;
  const totalAmount =
    subtotalAmount - discountAmount + shippingAmount + taxAmount;

  return prisma.$transaction(async (tx) => {
    const order = await tx.order.create({
      data: {
        orderNumber: createOrderNumber(),
        customerId: input.owner.userId,
        customerEmail: input.customerEmail,
        customerName: input.customerName,
        customerPhone: input.customerPhone,
        note: input.note,
        currencyCode: cart.currencyCode,
        subtotalAmount,
        discountAmount,
        shippingAmount,
        taxAmount,
        totalAmount,
        placedAt: new Date(),
      },
    });

    const orderItems = selectedCartItems.map((item) =>
      cartItemToOrderItemMapper(order.id, item),
    );

    await tx.orderItem.createMany({
      data: orderItems,
    });

    return tx.order.findUnique({
      where: {
        id: order.id,
      },
      include: {
        items: true,
      },
    });
  });
}

function getCartItemLineTotalAmount(item: CartItem) {
  if (item.lineTotalPreviewAmount === undefined) {
    throw new AppError(
      "INVALID_REQUEST_BODY",
      `Cart item ${item.variantId} is missing line total preview amount.`,
      400,
    );
  }

  return item.lineTotalPreviewAmount;
}

function createOrderNumber() {
  return `ORD-${Date.now()}-${randomUUID().slice(0, 8).toUpperCase()}`;
}
