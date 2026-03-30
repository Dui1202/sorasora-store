import SignUpForm from "./sign-up-form";
import AuthPageBackground from "@/components/auth-page-background";
import { requireGuest } from "@/lib/auth/authorization";

const SignUpPage = async () => {
  await requireGuest();

  return (
    <AuthPageBackground>
      <SignUpForm />
    </AuthPageBackground>
  );
};

export default SignUpPage;
