"use client";

import { Button } from "@/components/ui/button";

type ProductsErrorPageProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

const ProductsErrorPage = ({ error, reset }: ProductsErrorPageProps) => {
  return (
    <div className="p-5">
      <div className="space-y-2 rounded-lg border border-destructive/20 bg-destructive/5 p-4">
        <p className="text-sm font-medium text-destructive">
          Failed to load products.
        </p>
        <p className="text-sm text-muted-foreground">
          {error.message || "An unexpected error occurred while loading the product list."}
        </p>
        <Button type="button" variant="outline" onClick={reset}>
          Try again
        </Button>
      </div>
    </div>
  );
};

export default ProductsErrorPage;
