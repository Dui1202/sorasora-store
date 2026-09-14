import { SupplierCountry } from "@/lib/domain/products/supplier-country";
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
import {
  CreateProductFormInputData,
  UpdateProductFormInputData,
} from "@/lib/products/schema";
import { Controller, useFormContext } from "react-hook-form";
import type { FranchiseDto } from "../../dto/franchises-dto";
import type { WarehouseDto } from "../../dto/warehouses-dto";

type Props = {
  franchises?: FranchiseDto[];
  warehouses?: WarehouseDto[];
};

const SpecificationFieldGroup = ({ franchises = [], warehouses = [] }: Props) => {
  const { control } = useFormContext<
    CreateProductFormInputData | UpdateProductFormInputData
  >();
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
              items={franchises}
              disabled={franchises.length === 0}
              value={
                franchises?.find((franchise) => franchise.id === field.value) ??
                null
              }
              onValueChange={(franchise) => field.onChange(franchise?.id)}
              itemToStringLabel={(franchise) => franchise.name}
              itemToStringValue={(franchise) => franchise.id}
              isItemEqualToValue={(item, value) => item.id === value.id}
              name={field.name}
            >
              <ComboboxInput placeholder="Select a franchise" />
              <ComboboxContent>
                <ComboboxEmpty>No franchise found</ComboboxEmpty>
                <ComboboxList>
                  {(item: FranchiseDto) => (
                    <ComboboxItem key={item.id} value={item}>
                      {item.name}
                    </ComboboxItem>
                  )}
                </ComboboxList>
              </ComboboxContent>
            </Combobox>
          )}
          control={control}
          name="franchiseId"
        />
      </Field>
      <Field>
        <FieldLabel>Warehouse</FieldLabel>
        <Controller
          render={({ field }) => (
            <Combobox
              items={warehouses}
              disabled={warehouses.length === 0}
              value={
                warehouses?.find((warehouse) => warehouse.id === field.value) ??
                null
              }
              onValueChange={(warehouse) => field.onChange(warehouse?.id)}
              itemToStringLabel={(warehouse) => warehouse.name}
              itemToStringValue={(warehouse) => warehouse.id}
              isItemEqualToValue={(item, value) => item.id === value.id}
              name={field.name}
            >
              <ComboboxInput placeholder="Select a warehouse" />
              <ComboboxContent>
                <ComboboxEmpty>No warehouse found</ComboboxEmpty>
                <ComboboxList>
                  {(item: WarehouseDto) => (
                    <ComboboxItem key={item.id} value={item}>
                      {item.name}
                    </ComboboxItem>
                  )}
                </ComboboxList>
              </ComboboxContent>
            </Combobox>
          )}
          control={control}
          name="warehouseId"
        />
      </Field>
    </FieldGroup>
  );
};

export default SpecificationFieldGroup;
