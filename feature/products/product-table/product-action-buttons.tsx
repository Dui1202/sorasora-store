import { Button } from "@/components/ui/button";
import axios from "axios";
import React from "react";

type Props = {
  id: string;
};

const ProductActionButtons = ({ id }: Props) => {
  const onEditButtonClick = () => {};
  const onDelistButtonClick = () => {};
  const onDeleteButtonClick = async () => {
    await axios.delete(`/api/products/${id}`);
  };
  return (
    <div className="gap-2 flex">
      <Button variant="secondary">Edit</Button>
      <Button variant="secondary">Delist</Button>
      <Button variant="destructive">Delete</Button>
    </div>
  );
};

export default ProductActionButtons;
