"use client";

import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
import { routes } from "@/lib/routes";
import { ProductWithRelations } from "@/lib/validation-schema/product-schema";
import { useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";

type Props = {
  product: ProductWithRelations;
};

const ProductActionButtons = ({ product }: Props) => {
  const router = useRouter();
  const queryClient = useQueryClient();
  const id = product.id;
  const [isDeleting, setIsDeleting] = useState(false);
  const [isDelisting, setIsDelisting] = useState(false);
  const isPublished = product.isPublished;

  const onDeleteButtonClick = async () => {
    setIsDeleting(true);

    try {
      await axios.delete(`/api/products/${id}`);
      await queryClient.invalidateQueries({
        queryKey: ["products"],
      });
      toast.success("Product deleted successfully.");
    } catch {
      toast.error("Failed to delete product. Please try again.");
    } finally {
      setIsDeleting(false);
    }
  };

  const onDelistButtonClick = async () => {
    setIsDelisting(true);

    try {
      const command = {
        isPublished: !isPublished,
      };

      await axios.patch(`/api/products/${id}`, command);
      router.refresh();
      toast.success(
        `Product ${isPublished ? "delisted" : "published"} successfully.`,
      );
    } catch {
      toast.error(
        `Failed to ${isPublished ? "delist" : "publish"} product. Please try again.`,
      );
    } finally {
      setIsDelisting(false);
    }
  };

  return (
    <div className="gap-2 flex flex-col">
      <Button variant="secondary" asChild disabled={isDeleting || isDelisting}>
        <Link href={routes.admin.productEdit(id)}>Edit</Link>
      </Button>
      <Button
        variant="secondary"
        onClick={onDelistButtonClick}
        disabled={isDelisting || isDeleting}
      >
        {isPublished ? "Delist" : "Publish"} {isDelisting && <Spinner />}
      </Button>
      <Button
        variant="destructive"
        onClick={onDeleteButtonClick}
        disabled={isDeleting || isDelisting}
      >
        Delete {isDeleting && <Spinner />}
      </Button>
    </div>
  );
};

export default ProductActionButtons;
