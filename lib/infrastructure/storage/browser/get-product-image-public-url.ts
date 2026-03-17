import { supabase } from "./supabase";

export function getProductImagePublicUrl(path: string) {
  const { data } = supabase.storage.from("product-images").getPublicUrl(path);

  return data.publicUrl;
}
