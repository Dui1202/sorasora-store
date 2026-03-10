import { supabase } from "../supabase";

export async function cleanUpTempProductImage(
  imageId: string,
  draftsId: string,
) {
  const filePath = `products/temp/${draftsId}/${imageId}`;
  const { error: deleteError } = await supabase.storage
    .from("product-images")
    .remove([filePath]);
  if (deleteError) {
    throw deleteError;
  }
}
