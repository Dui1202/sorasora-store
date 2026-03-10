"use client";

import {
  AlertDialogTrigger,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogCancel,
  AlertDialogAction,
  AlertDialog,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useRouter } from "next/navigation";
import axios from "axios";
const FormButton = () => {
  const router = useRouter();

  const onContinueButtonClick = async () => {
    router.push("/products");
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
      <Button type="submit" variant="secondary">
        Save and Delist
      </Button>
      <Button type="submit">Save and Publish</Button>
    </Card>
  );
};

export default FormButton;
