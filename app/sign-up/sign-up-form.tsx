"use client";

import { ErrorMessage, TypographyH2 } from "@/components/typography";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/components/ui/card";
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { authClient } from "@/lib/auth/auth-client";
import {
  signUpSchema,
  SignUpSchema,
} from "@/lib/validation-schema/sign-up-schema";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

const SignUpForm = () => {
  const router = useRouter();
  const methods = useForm<SignUpSchema>({
    resolver: zodResolver(signUpSchema),
    defaultValues: {
      name: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
  });
  const { register, handleSubmit, formState } = methods;

  const onSubmit = async (data: SignUpSchema) => {
    try {
      const result = await authClient.signUp.email({
        email: data.email,
        password: data.password,
        name: data.name,
      });

      if (result.error) {
        toast.error(result.error.message ?? "Failed to sign up.");
        return;
      }

      router.push("/");
      router.refresh();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unexpected error happened.";
      toast.error(message);
    }
  };
  return (
    <Card className="w-full sm:max-w-md bg-white/70 backdrop-blur-md p-8 rounded-2xl shadow-lg">
      <CardHeader>
        <TypographyH2>Sign Up</TypographyH2>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldSet>
            <FieldGroup>
              <Field>
                <FieldLabel>User Name</FieldLabel>
                <FieldDescription>A name for your account</FieldDescription>

                <Input
                  {...register("name")}
                  placeholder="User Name..."
                  autoComplete="name"
                />
                {formState.errors.name && (
                  <ErrorMessage>{formState.errors.name.message}</ErrorMessage>
                )}
              </Field>
              <Field>
                <FieldLabel>Email</FieldLabel>
                <FieldDescription>Your email address</FieldDescription>

                <Input
                  {...register("email")}
                  placeholder="Email..."
                  type="email"
                  autoComplete="email"
                />
                {formState.errors.email && (
                  <ErrorMessage>{formState.errors.email.message}</ErrorMessage>
                )}
              </Field>
              <Field>
                <FieldLabel>Password</FieldLabel>
                <FieldDescription>At least 8 characters</FieldDescription>

                <Input
                  {...register("password")}
                  placeholder="Password..."
                  type="password"
                  autoComplete="new-password"
                />
                {formState.errors.password && (
                  <ErrorMessage>
                    {formState.errors.password.message}
                  </ErrorMessage>
                )}
              </Field>
              <Field>
                <FieldLabel>Confirm Password</FieldLabel>
                <Input
                  {...register("confirmPassword")}
                  placeholder="Confirm Password..."
                  type="password"
                  autoComplete="new-password"
                />
                {formState.errors.confirmPassword && (
                  <ErrorMessage>
                    {formState.errors.confirmPassword.message}
                  </ErrorMessage>
                )}
              </Field>
            </FieldGroup>
          </FieldSet>
          <div className="mt-10">
            <Button
              type="submit"
              className="w-full"
              disabled={formState.isSubmitting}
            >
              Sign Up
              {formState.isSubmitting && <Spinner />}
            </Button>
          </div>
        </form>
      </CardContent>
      <CardFooter>
        <p>
          Already have an account?{" "}
          <Link href={"/sign-in"} className="text-primary underline">
            Sign in
          </Link>
        </p>
      </CardFooter>
    </Card>
  );
};

export default SignUpForm;
