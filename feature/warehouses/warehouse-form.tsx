"use client";

import SectionCard from "@/components/section-card";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
  Field,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Separator } from "@/components/ui/separator";
import { routes } from "@/lib/routes";
import { Warehouse } from "@/lib/warehouse/schema";
import { Heading } from "@radix-ui/themes";
import UnavailableNotice from "@/components/unavailable-notice";
import { useRouter } from "next/navigation";
import { FormProvider, useForm } from "react-hook-form";

const WarehouseForm = () => {
  const methods = useForm<Warehouse>();
  const { register } = methods;
  const router = useRouter();

  const onContinueButtonClick = () => {
    router.push(routes.admin.warehouses);
  };

  return (
    <FormProvider {...methods}>
      <form className="p-5" onSubmit={(event) => event.preventDefault()}>
        <UnavailableNotice feature="Warehouse creation" />
        <fieldset disabled>
          <Heading>Add Warehouse</Heading>
          <Separator className="my-2" />
          <SectionCard>
            <FieldSet>
              <FieldLegend>Basic Information</FieldLegend>
              <Field>
                <FieldLabel>
                  Warehouse Name <span className="text-destructive">*</span>
                </FieldLabel>
                <Input
                  placeholder="Warehouse Name..."
                  {...register("name")}
                  required
                />
              </Field>
            </FieldSet>
          </SectionCard>
          <Card className="flex flex-row justify-end gap-3 sticky bottom-0 p-5">
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button variant="secondary">Cancel</Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Confirmation</AlertDialogTitle>
                  <AlertDialogDescription>
                    Discard changes?
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={onContinueButtonClick}>
                    Continue
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
            <Button type="submit">Add Warehouse</Button>
          </Card>
        </fieldset>
      </form>
    </FormProvider>
  );
};

export default WarehouseForm;
