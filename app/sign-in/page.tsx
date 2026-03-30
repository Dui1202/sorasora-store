import SignInForm from "./sign-in-form";
import AuthPageBackground from "@/components/auth-page-background";
import { requireGuest } from "@/lib/auth/authorization";

const SignInPage = async () => {
  await requireGuest();

  return (
    <AuthPageBackground>
      <SignInForm />
    </AuthPageBackground>
  );
};

export default SignInPage;
