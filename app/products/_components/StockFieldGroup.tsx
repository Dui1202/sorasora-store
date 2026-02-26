import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";

const StockFieldGroup = () => {
  return (
    <FieldGroup className="grid grid-cols-2">
      <Field className="col-span-1">
        <FieldLabel>Stock</FieldLabel>
        <Input placeholder="Stock..." />
      </Field>
    </FieldGroup>
  );
};

export default StockFieldGroup;
