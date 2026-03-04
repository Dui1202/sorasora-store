import prisma from "@/lib/prisma";
import { NextRequest, NextResponse } from "next/server";

export async function GET(request: NextRequest) {
  const franchises = await prisma.franchise.findMany({
    select: {
      id: true,
      name: true,
    },
    orderBy: { name: "asc" },
  });

  return NextResponse.json(franchises);
}
