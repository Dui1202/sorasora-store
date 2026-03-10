import { supabase } from "../supabase";

export async function removeProductImage(imageId: string) {
  const filePath = `products/product-images/${imageId}`;
  const { error: deleteError } = await supabase.storage
    .from("product-images")
    .remove([filePath]);
  if (deleteError) {
    throw deleteError;
  }
}
