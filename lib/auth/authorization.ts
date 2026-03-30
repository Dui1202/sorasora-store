import "server-only";

import { cookies, headers } from "next/headers";
import { redirect } from "next/navigation";
import type { NextRequest } from "next/server";
import { auth } from "@/lib/auth/auth";
import type { UserRole } from "@/app/generated/prisma/enums";
import { AppError } from "@/core/errors/errors";
import { hasSessionCookie } from "@/lib/auth/session-cookie";
import { routes } from "@/lib/routes";

export async function requireSession() {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (!session) {
    redirect(routes.auth.signIn);
  }

  return session;
}

export async function requireGuest() {
  const cookieStore = await cookies();
  const hasExistingSessionCookie = hasSessionCookie(cookieStore);

  if (!hasExistingSessionCookie) {
    return null;
  }

  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (session) {
    redirect(routes.root);
  }

  return session;
}

export async function requireRole(allowedRoles: UserRole[]) {
  const session = await requireSession();
  const userRole = session.user.role as UserRole | undefined;

  if (!userRole || !allowedRoles.includes(userRole)) {
    redirect(routes.root);
  }

  return session;
}

export async function requireApiRole(
  request: NextRequest,
  allowedRoles: UserRole[],
) {
  const session = await auth.api.getSession({
    headers: request.headers,
  });
  const userRole = session?.user.role as UserRole | undefined;

  if (!session) {
    throw new AppError("AUTHENTICATION_ERROR", "Authentication required", 401);
  }

  if (!userRole || !allowedRoles.includes(userRole)) {
    throw new AppError("AUTHORIZATION_ERROR", "Forbidden", 403);
  }

  return session;
}
