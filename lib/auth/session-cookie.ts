const SESSION_COOKIE_NAMES = [
  "better-auth.session_token",
  "__Secure-better-auth.session_token",
] as const;

type CookieStore = {
  has(name: string): boolean;
};

export function hasSessionCookie(cookieStore: CookieStore) {
  return SESSION_COOKIE_NAMES.some((cookieName) => cookieStore.has(cookieName));
}
