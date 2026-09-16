import { api } from "../api/client";
import { CreateWarehouseRequest, UpdateWarehouseRequest } from "./schema";

export type WarehouseResponse = {
  id: string;
  name: string;
  isDefault: boolean;
  isArchive: boolean;
};

const WAREHOUSE_ENDPOINT = "api/warehouses";

export async function getWarehouses() {
  return await api.get<WarehouseResponse[]>(WAREHOUSE_ENDPOINT);
}

export async function getWarehouseById(id: string) {
  return await api.get<WarehouseResponse>(`${WAREHOUSE_ENDPOINT}/${id}`);
}

export async function createWarehouse(input: CreateWarehouseRequest) {
  return await api.post<WarehouseResponse>(WAREHOUSE_ENDPOINT, input);
}

export async function updateWarehouse(input: UpdateWarehouseRequest) {
  return await api.put<WarehouseResponse>(WAREHOUSE_ENDPOINT, input);
}
