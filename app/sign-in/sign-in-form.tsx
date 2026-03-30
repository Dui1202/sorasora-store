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
import { routes } from "@/lib/routes";
import {
  signInSchema,
  SignInSchema,
} from "@/lib/validation-schema/sign-in-schema";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useRouter } from "next/navigation";

import { useForm } from "react-hook-form";
import { toast } from "sonner";

const SignInForm = () => {
  const router = useRouter();
  const { register, formState, handleSubmit } = useForm<SignInSchema>({
    resolver: zodResolver(signInSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const onSubmit = async (data: SignInSchema) => {
    try {
      const result = await authClient.signIn.email({
        email: data.email,
        password: data.password,
        rememberMe: true,
      });

      if (result.error) {
        toast.error(result.error.message ?? "Failed to sign in.");
        return;
      }

      router.push(routes.admin.root);
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
        <TypographyH2>Sign In</TypographyH2>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit(onSubmit)}>
          <FieldSet>
            <FieldGroup>
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

                <Input
                  {...register("password")}
                  placeholder="Password..."
                  type="password"
                  autoComplete="current-password"
                />
                {formState.errors.password && (
                  <ErrorMessage>
                    {formState.errors.password.message}
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
              Sign In
              {formState.isSubmitting && <Spinner />}
            </Button>
          </div>
        </form>
      </CardContent>
      <CardFooter>
        <p>
          Don&apos;t have an account?{" "}
          <Link href={routes.auth.signUp} className="text-primary underline">
            Sign up
          </Link>
        </p>
      </CardFooter>
    </Card>
  );
};

export default SignInForm;
