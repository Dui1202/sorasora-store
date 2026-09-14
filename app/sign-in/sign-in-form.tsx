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
import UnavailableNotice from "@/components/unavailable-notice";
import { routes } from "@/lib/routes";
import {
  signInSchema,
  SignInSchema,
} from "@/lib/validation-schema/sign-in-schema";
import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";

import { useForm } from "react-hook-form";

const SignInForm = () => {
  const { register, formState } = useForm<SignInSchema>({
    resolver: zodResolver(signInSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  return (
    <Card className="w-full sm:max-w-md bg-white/70 backdrop-blur-md p-8 rounded-2xl shadow-lg">
      <CardHeader>
        <TypographyH2>Sign In</TypographyH2>
      </CardHeader>
      <CardContent>
        <UnavailableNotice feature="Sign-in" />
        <form onSubmit={(event) => event.preventDefault()}>
          <fieldset disabled>
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
          </fieldset>
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
