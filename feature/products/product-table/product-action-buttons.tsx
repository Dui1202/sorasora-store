import { Button } from "@/components/ui/button";
import axios from "axios";
import Link from "next/link";
import { useRouter } from "next/navigation";
import React from "react";

type Props = {
  id: string;
};

const ProductActionButtons = ({ id }: Props) => {
  const onDelistButtonClick = () => {};
  const onDeleteButtonClick = async () => {
    await axios.delete(`/api/products/${id}`);
  };
  return (
    <div className="gap-2 flex flex-col">
      <Button variant="secondary">
        <Link href={`/products/${id}/edit`}>Edit</Link>
      </Button>
      <Button variant="secondary">Delist</Button>
      <Button variant="destructive">Delete</Button>
    </div>
  );
};

export default ProductActionButtons;
