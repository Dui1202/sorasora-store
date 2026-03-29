import { auth } from "@/lib/auth/auth";
import SignUpForm from "./sign-up-form";
import { headers } from "next/headers";
import { redirect } from "next/navigation";
import AuthPageBackground from "@/components/auth-page-background";

const SignUpPage = async () => {
  const session = await auth.api.getSession({
    headers: await headers(),
  });

  if (session) {
    redirect("/");
  }
  return (
    <AuthPageBackground>
      <SignUpForm />
    </AuthPageBackground>
  );
};

export default SignUpPage;
