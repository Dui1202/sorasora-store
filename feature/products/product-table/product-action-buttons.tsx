"use client";

import { Button } from "@/components/ui/button";
import { useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import Link from "next/link";
import { useState } from "react";
import { toast } from "sonner";

type Props = {
  id: string;
};

const ProductActionButtons = ({ id }: Props) => {
  const queryClient = useQueryClient();
  const [isDeleting, setIsDeleting] = useState(false);

  const onDeleteButtonClick = async () => {
    setIsDeleting(true);

    try {
      await axios.delete(`/api/products/${id}`);
      await queryClient.invalidateQueries({
        queryKey: ["products"],
      });
      toast.success("Product archived successfully.");
    } catch {
      toast.error("Failed to archive product. Please try again.");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="gap-2 flex flex-col">
      <Button variant="secondary">
        <Link href={`/products/${id}/edit`}>Edit</Link>
      </Button>
      <Button variant="secondary">Delist</Button>
      <Button
        variant="destructive"
        onClick={onDeleteButtonClick}
        disabled={isDeleting}
      >
        {isDeleting ? "Archiving..." : "Delete"}
      </Button>
    </div>
  );
};

export default ProductActionButtons;
