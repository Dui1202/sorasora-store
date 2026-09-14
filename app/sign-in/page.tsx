import SignInForm from "./sign-in-form";
import AuthPageBackground from "@/components/auth-page-background";

const SignInPage = async () => {
  return (
    <AuthPageBackground>
      <SignInForm />
    </AuthPageBackground>
  );
};

export default SignInPage;
