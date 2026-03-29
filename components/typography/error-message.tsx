import React from "react";

type Props = {
  children: React.ReactNode;
  className?: string;
};
export function ErrorMessage({ children, className }: Props) {
  return <p className={`text-sm text-destructive ${className}`}>{children}</p>;
}

export default ErrorMessage;
