import { removeProductImages } from "./removeProductImages";

export async function removeProductImage(path: string) {
  await removeProductImages([path]);
}
