import { UserRole } from "@/app/generated/prisma/enums";
import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import { requireApiRole } from "@/lib/auth/authorization";
import { warehouseBaseSchema } from "@/lib/warehouse/schema";
import { NextRequest, NextResponse } from "next/server";
import z from "zod";

const ADMIN_ROLES = [UserRole.STAFF, UserRole.ADMIN];

export async function GET(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

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
  } catch (error) {
    return toErrorResponse(error);
  }
}

export async function POST(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

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
  } catch (error) {
    return toErrorResponse(error);
  }
}
