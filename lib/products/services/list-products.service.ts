import {
  findProductById,
  findProducts,
} from "../repositories/product.repository";

export async function listProductsService() {
  return findProducts();
}

export async function getProductWithRelationsByIdService(id: string) {
  return findProductById(id, true);
}
