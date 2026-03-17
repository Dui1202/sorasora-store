import { supabase } from "./supabase";

export async function removeProductImages(paths: string[]) {
  if (paths.length === 0) {
    return;
  }

  const { error: deleteError } = await supabase.storage
    .from("product-images")
    .remove(paths);

  if (deleteError) {
    throw deleteError;
  }
}
