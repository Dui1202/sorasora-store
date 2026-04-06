"use client";

import axios from "axios";
import { toCreateProductMultipartFormData } from "@/lib/products/mappers/product-command.mapper";
import { CreateProductFormData } from "@/lib/validation-schema/product-schema";
import ProductForm from "@/feature/products/product-form/product-form";
import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { routes } from "@/lib/routes";

const CreateNewProductPage = () => {
  const router = useRouter();
  const onSubmit = async (data: CreateProductFormData) => {
    const multipartFormData = toCreateProductMultipartFormData(data);
    try {
      await axios.post("/api/products", multipartFormData);

      router.push(routes.admin.products);
      toast.success("Product created successfully.");
    } catch (error) {
      toast.error("Failed to create product. Please try again.");
    }
  };

  return <ProductForm mode="create" onSubmit={onSubmit} />;
};

export default CreateNewProductPage;
