import "server-only";

import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { auth } from "@/lib/auth/auth";
import type { UserRole } from "@/app/generated/prisma/enums";

export async function requireSession() {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (!session) {
    redirect("/sign-in");
  }

  return session;
}

export async function requireGuest() {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (session) {
    redirect("/");
  }

  return session;
}

export async function requireRole(allowedRoles: UserRole[]) {
  const session = await requireSession();
  const userRole = session.user.role as UserRole | undefined;

  if (!userRole || !allowedRoles.includes(userRole)) {
    redirect("/");
  }

  return session;
}
