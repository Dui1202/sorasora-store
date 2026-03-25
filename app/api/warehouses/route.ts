import prisma from "@/lib/infrastructure/database/prisma";
import { warehouseBaseSchema } from "@/lib/validation-schema/warehouse-schema";
import { NextRequest, NextResponse } from "next/server";
import z from "zod";

export async function GET() {
  const warehouses = await prisma.warehouse.findMany({
    select: {
      id: true,
      name: true,
    },
    orderBy: {
      name: "asc",
    },
  });

  return NextResponse.json(warehouses);
}

export async function POST(request: NextRequest) {
  const body = await request.json();

  const validationResult = warehouseBaseSchema.safeParse(body);

  if (!validationResult.success) {
    return NextResponse.json(
      {
        error: "Validation failed",
        issues: z.treeifyError(validationResult.error),
      },
      { status: 400 },
    );
  }

  const { name } = validationResult.data;

  const newWareHouse = await prisma.warehouse.create({
    data: {
      name,
    },
  });

  return NextResponse.json(newWareHouse, { status: 201 });
}
