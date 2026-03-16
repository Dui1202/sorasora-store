import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { FranchiseDto, franchiseListDtoSchema } from "../../dto/franchises-dto";

export function useFranchises() {
  return useQuery<FranchiseDto[]>({
    queryKey: ["franchises"],
    queryFn: async () => {
      const res = await axios.get("/api/franchises");
      return franchiseListDtoSchema.parse(res.data);
    },
    staleTime: 24 * 60 * 60 * 1000,
    retry: 3,
  });
}
