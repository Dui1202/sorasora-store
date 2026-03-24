import {
  findProductById,
  findProducts,
} from "../repositories/product.repository";

export async function listProductsService() {
  return findProducts();
}
export async function getProductByIdService(id: string, hasRelations: boolean) {
  return findProductById(id, hasRelations);
}
