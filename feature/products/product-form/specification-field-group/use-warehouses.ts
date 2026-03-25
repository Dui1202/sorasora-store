import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { WarehouseDto, warehouseListDtoSchema } from "../../dto/warehouses-dto";

export function useWarehouses() {
  return useQuery<WarehouseDto[]>({
    queryKey: ["warehouses"],
    queryFn: async () => {
      const res = await axios.get("/api/warehouses");
      return warehouseListDtoSchema.parse(res.data);
    },
    staleTime: 24 * 60 * 60 * 1000,
    retry: 3,
  });
}
