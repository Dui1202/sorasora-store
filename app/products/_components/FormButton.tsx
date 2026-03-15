"use client";

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
import { Spinner } from "@/components/ui/spinner";
import { CreateProductFormInputData } from "@/lib/validation-schema/product-schema/productSchema";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useFormContext } from "react-hook-form";

type FormButtonProps = {
  isSubmitting: boolean;
};

const FormButton = ({ isSubmitting }: FormButtonProps) => {
  const router = useRouter();
  const [isPublishButtonClicked, setIsPublishButtonClicked] = useState(false);

  const onContinueButtonClick = () => {
    router.push("/products");
  };
  const { register, setValue } = useFormContext<CreateProductFormInputData>();
  useEffect(() => {
    register("isPublished");
  }, [register]);

  const onPublishButtonClick = () => {
    setValue("isPublished", true, {
      shouldDirty: true,
    });
    setIsPublishButtonClicked(true);
  };

  const onDelistButtonClick = () => {
    setValue("isPublished", false, {
      shouldDirty: true,
    });
    setIsPublishButtonClicked(false);
  };

  return (
    <Card className="flex flex-row justify-end gap-3 sticky bottom-0 p-5">
      <AlertDialog>
        <AlertDialogTrigger asChild>
          <Button variant="secondary">Cancel</Button>
        </AlertDialogTrigger>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirmation</AlertDialogTitle>
            <AlertDialogDescription>Discard changes?</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={onContinueButtonClick}>
              Continue
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <Button
        type="submit"
        variant="secondary"
        onClick={onDelistButtonClick}
        disabled={isSubmitting}
      >
        Save and Delist
        {isSubmitting && !isPublishButtonClicked && <Spinner />}
      </Button>
      <Button
        type="submit"
        onClick={onPublishButtonClick}
        disabled={isSubmitting}
      >
        Save and Publish
        {isSubmitting && isPublishButtonClicked && <Spinner />}
      </Button>
    </Card>
  );
};

export default FormButton;
