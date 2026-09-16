"use client";

import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { routes } from "@/lib/routes";
import {
  CreateWarehouseRequest,
  createWarehouseRequestSchema,
} from "@/lib/warehouse/schema";
import { useRouter } from "next/navigation";
import { Controller, FormProvider, useForm } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { zodResolver } from "@hookform/resolvers/zod";
import { createWarehouse } from "@/lib/warehouse/api";
import { warehouseKeys } from "@/lib/warehouse/query-keys";
import { toast } from "sonner";
import { Checkbox } from "@/components/ui/checkbox";
import SectionCard from "@/components/section-card";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/spinner";
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

const WarehouseForm = () => {
  const router = useRouter();
  const queryClient = useQueryClient();

  const methods = useForm<CreateWarehouseRequest>({
    resolver: zodResolver(createWarehouseRequestSchema),
    defaultValues: {
      name: "",
      isDefault: false,
    },
  });
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = methods;

  const createMutation = useMutation({
    mutationFn: createWarehouse,

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: warehouseKeys.all,
      });

      toast.success("Warehouse created!");

      router.push(routes.admin.warehouses);
    },

    onError: () => {
      toast.error("Failed to create warehouse.");
    },
  });

  const onSubmit = (request: CreateWarehouseRequest) => {
    createMutation.mutate(request);
  };
  return (
    <FormProvider {...methods}>
      <form className="p-5 " onSubmit={handleSubmit(onSubmit)}>
        <h1>Create Warehouse</h1>
        <Separator className="my-2" />
        <SectionCard>
          <Field>
            <FieldLabel>Warehouse Name</FieldLabel>
            <Input placeholder="Enter name" {...register("name")} />
            {errors.name && <FieldError>{errors.name.message}</FieldError>}
          </Field>
          <Field orientation="horizontal">
            <Controller
              control={methods.control}
              name="isDefault"
              render={({ field }) => (
                <Checkbox
                  id="isDefault"
                  onCheckedChange={field.onChange}
                  checked={field.value}
                />
              )}
            />
            <FieldLabel htmlFor="isDefault">Set as Default</FieldLabel>
          </Field>
        </SectionCard>
        <SectionCard className="flex flex-row justify-end gap-2">
          <AlertDialog>
            <AlertDialogTrigger
              render={
                <Button variant={"secondary"} disabled={isSubmitting}>
                  Cancel
                </Button>
              }
            />
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Confirmation</AlertDialogTitle>
                <AlertDialogDescription>
                  Discard changes?
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={() => {
                    router.push("/admin");
                  }}
                >
                  Confirm
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>

          <Button type="submit" disabled={isSubmitting}>
            Create {isSubmitting && <Spinner />}
          </Button>
        </SectionCard>
      </form>
    </FormProvider>
  );
};

export default WarehouseForm;
