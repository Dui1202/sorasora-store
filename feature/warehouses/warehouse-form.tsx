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
import axios from "axios";
import { useRouter } from "next/navigation";
import { FormProvider, SubmitHandler, useForm } from "react-hook-form";
import { toast } from "sonner";

const WarehouseForm = () => {
  const methods = useForm<Warehouse>();
  const { register, handleSubmit } = methods;
  const router = useRouter();

  const submitWarehouse: SubmitHandler<Warehouse> = async (data) => {
    try {
      await axios.post("/api/warehouses", data);
    } catch {
      toast.error("Failed to create warehouse. Please try again.", {
        position: "top-center",
      });
      return;
    }
    toast.success("Warehouse created successfully!", {
      position: "top-center",
    });
  };

  const onContinueButtonClick = () => {
    router.push(routes.admin.warehouses);
  };

  return (
    <FormProvider {...methods}>
      <form className="p-5" onSubmit={handleSubmit(submitWarehouse)}>
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
      </form>
    </FormProvider>
  );
};

export default WarehouseForm;
