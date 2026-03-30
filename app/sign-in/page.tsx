import { headers } from "next/headers";
import SignInForm from "./sign-in-form";
import { auth } from "@/lib/auth/auth";
import { redirect } from "next/navigation";
import AuthPageBackground from "@/components/auth-page-background";
import { routes } from "@/lib/routes";

const SignInPage = async () => {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (session) {
    redirect(routes.admin.root);
  }
  return (
    <AuthPageBackground>
      <SignInForm />
    </AuthPageBackground>
  );
};

export default SignInPage;
