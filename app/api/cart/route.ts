import { toErrorResponse } from "@/core/errors/http";
import { getCurrentCartOwner } from "@/lib/cart/http/get-current-cart-owner.http";
import { getCartService } from "@/lib/cart/service/get-cart.service";
import { NextRequest, NextResponse } from "next/server";

export async function GET(_request: NextRequest) {
  try {
    const owner = await getCurrentCartOwner();
    if (!owner) {
      return NextResponse.json(null, { status: 200 });
    }
    const cart = await getCartService(owner);
    return NextResponse.json(cart, { status: 200 });
  } catch (error) {
    toErrorResponse(error);
  }
}
