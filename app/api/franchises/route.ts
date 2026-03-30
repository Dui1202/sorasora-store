import { UserRole } from "@/app/generated/prisma/enums";
import { toErrorResponse } from "@/core/errors/http";
import prisma from "@/lib/infrastructure/database/prisma";
import { requireApiRole } from "@/lib/auth/authorization";
import { NextRequest, NextResponse } from "next/server";

const ADMIN_ROLES = [UserRole.STAFF, UserRole.ADMIN];

export async function GET(request: NextRequest) {
  try {
    await requireApiRole(request, ADMIN_ROLES);

    const franchises = await prisma.franchise.findMany({
      select: {
        id: true,
        name: true,
      },
      orderBy: { name: "asc" },
    });

    return NextResponse.json(franchises);
  } catch (error) {
    return toErrorResponse(error);
  }
}
