import { supabase } from "../supabase";

export async function cleanUpTempProductVariantImage(draftsId: string) {
  const folderPath = `products/temp/${draftsId}`;
  const { data: imageFiles, error: listError } = await supabase.storage
    .from("product-images")
    .list(folderPath);

  if (listError) {
    throw listError;
  }

  if (!imageFiles || imageFiles.length === 0) {
    return;
  }

  const imageFilePaths = imageFiles
    .map((file) => file.name)
    .filter(Boolean)
    .map((fileName) => `${folderPath}/${fileName}`);

  if (imageFilePaths.length === 0) {
    return;
  }

  const { error: deleteError } = await supabase.storage
    .from("product-images")
    .remove(imageFilePaths);

  if (deleteError) {
    throw deleteError;
  }
}
