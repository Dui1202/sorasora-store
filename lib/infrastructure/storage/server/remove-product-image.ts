import { removeProductImages } from "./remove-product-images";

export async function removeProductImage(path: string) {
  await removeProductImages([path]);
}
