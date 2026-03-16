import { supabase } from "./supabase";

export async function getProductImagePublicUrl(path: string) {
  const { data } = await supabase.storage
    .from("product-images")
    .getPublicUrl(path);

  return data.publicUrl;
}
