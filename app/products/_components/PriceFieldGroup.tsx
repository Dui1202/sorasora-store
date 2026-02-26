import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

const PriceFieldGroup = () => {
  return (
    <FieldGroup className="flex flex-row">
      <Field>
        <FieldLabel>Original Price</FieldLabel>
        <FieldDescription>
          Orginal price in the supplier country's currency
        </FieldDescription>
        <Input placeholder="Orginal price..." />
      </Field>
      <Field>
        <FieldLabel>Sale Price</FieldLabel>
        <FieldDescription>Price you want to sale for</FieldDescription>
        <Input placeholder="Sale price..." />
      </Field>
    </FieldGroup>
  );
};

export default PriceFieldGroup;
