import SignUpForm from "./sign-up-form";
import AuthPageBackground from "@/components/auth-page-background";

const SignUpPage = async () => {
  return (
    <AuthPageBackground>
      <SignUpForm />
    </AuthPageBackground>
  );
};

export default SignUpPage;
