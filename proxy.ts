// proxy.ts
import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { routes } from "@/lib/routes";

export function proxy(req: NextRequest) {
  const hasSession = req.cookies.get("better-auth.session");

  if (!hasSession && req.nextUrl.pathname.startsWith(routes.admin.root)) {
    return NextResponse.redirect(new URL(routes.auth.signIn, req.url));
  }

  return NextResponse.next();
}
