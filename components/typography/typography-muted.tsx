import { ReactNode } from "react";

type TypographyMutedProps = {
  children: ReactNode;
  className?: string;
};

export function TypographyMuted({ children, className }: TypographyMutedProps) {
  return (
    <p className={`text-sm text-muted-foreground ${className ?? ""}`}>
      {children}
    </p>
  );
}
