import { betterAuth } from "better-auth";
import { prismaAdapter } from "better-auth/adapters/prisma";
import prisma from "../infrastructure/database/prisma";
import { nextCookies } from "better-auth/next-js";
import { admin } from "better-auth/plugins";

export const auth = betterAuth({
  database: prismaAdapter(prisma, {
    provider: "postgresql",
  }),
  emailAndPassword: { enabled: true },
  user: {
    additionalFields: {
      role: {
        type: ["CUSTOMER", "STAFF", "ADMIN"],
        required: false,
        defaultValue: "CUSTOMER",
        input: false,
      },
    },
  },
  secret: process.env.BETTER_AUTH_SECRET!,
  cookies: [nextCookies()],
  plugins: [admin()],
});
