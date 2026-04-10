import { ReactNode } from "react";

type TypographyH2Props = {
  children: ReactNode;
  className?: string;
};

export function TypographyH2({ children, className }: TypographyH2Props) {
  return (
    <h2
      className={`scroll-m-20 text-3xl font-semibold tracking-tight first:mt-0 ${className ?? ""}`}
    >
      {children}
    </h2>
  );
}
