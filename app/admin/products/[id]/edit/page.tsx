import UpdateProductFormClient from "@/feature/products/product-form/update-product-form-client";
import { getProductWithRelationsByIdService } from "@/lib/products";
import { notFound } from "next/navigation";

type Props = {
  params: Promise<{ id: string }>;
};

const EditProductPage = async ({ params }: Props) => {
  const { id } = await params;
  const product = await getProductWithRelationsByIdService(id);

  if (!product) {
    notFound();
  }

  return <UpdateProductFormClient product={product} />;
};

export default EditProductPage;
