import { SupplierCountry } from "@/app/generated/prisma/enums";
import {
  Combobox,
  ComboboxContent,
  ComboboxEmpty,
  ComboboxInput,
  ComboboxItem,
  ComboboxList,
} from "@/components/ui/combobox";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/validationSchema";
import { Controller, useFormContext } from "react-hook-form";
import type { FranchiseDto } from "../../dto/franchises.dto";
import { useFranchises } from "./useFranchises";
import { Button } from "@/components/ui/button";

const SpecificationFieldGroup = () => {
  const { control } = useFormContext<CreateProductFormInputData>();
  const {
    data: franchises,
    isLoading: isLoadingFranchises,
    isError: isFranchisesError,
    refetch: refetchFranchises,
  } = useFranchises();

  return (
    <FieldGroup className="flex flex-row">
      <Field>
        <FieldLabel>Supplier Country</FieldLabel>
        <Controller
          render={({ field }) => (
            <Select
              value={field.value}
              onValueChange={(v) => field.onChange(v || undefined)}
              name={field.name}
            >
              <SelectTrigger>
                <SelectValue placeholder="Choose a country..." />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>Country</SelectLabel>
                  <SelectItem value={SupplierCountry.JAPAN}>Japan</SelectItem>
                  <SelectItem value={SupplierCountry.CHINA}>China</SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
          )}
          name="supplierCountry"
          control={control}
        />
      </Field>
      <Field>
        <FieldLabel>Franchise</FieldLabel>
        <Controller
          render={({ field }) => (
            <Combobox
              items={franchises ?? []}
              value={field.value}
              onValueChange={field.onChange}
              name={field.name}
            >
              <ComboboxInput placeholder="Select a franchise" />
              <ComboboxContent>
                <ComboboxEmpty>No franchise found</ComboboxEmpty>
                {isLoadingFranchises ? (
                  <Skeleton />
                ) : isFranchisesError ? (
                  <div className="p-2 text-sm text-red-600">
                    Failed to load franchises.
                    <Button
                      type="button"
                      onClick={() => refetchFranchises()}
                      className="ml-2 underline"
                    >
                      Retry
                    </Button>
                  </div>
                ) : (
                  <ComboboxList>
                    {(item: FranchiseDto) => (
                      <ComboboxItem key={item.id} value={item.id}>
                        {item.name}
                      </ComboboxItem>
                    )}
                  </ComboboxList>
                )}
              </ComboboxContent>
            </Combobox>
          )}
          control={control}
          name="franchiseId"
        />
      </Field>
    </FieldGroup>
  );
};

export default SpecificationFieldGroup;
