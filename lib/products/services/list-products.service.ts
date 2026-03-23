import { findProducts } from "../repositories/product.repository";

export async function listProductsService() {
  return findProducts();
}
