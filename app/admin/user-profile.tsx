"use client";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { Spinner } from "@/components/ui/spinner";
import { authClient } from "@/lib/auth/auth-client";
import { useDelay } from "@/lib/dev/use-delay";
import { CircleUserRound, LogOutIcon } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";

const UserProfile = () => {
  const router = useRouter();
  const [isSignOutPending, setIsSignOutPending] = useState(false);
  const { data, isPending } = authClient.useSession();

  const isDelayed = useDelay(1000);

  const handleSignOut = async () => {
    try {
      setIsSignOutPending(true);

      const result = await authClient.signOut();
      if (result.error) {
        toast.error(result.error.message ?? "Failed to sign out.");
        return;
      }
      router.push("/sign-in");
      router.refresh();
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Unexpected error happened.";
      toast.error(message);
    } finally {
      setIsSignOutPending(false);
    }
  };

  if (isPending || isDelayed) {
    return (
      <div className="flex w-fit items-center gap-3 p-1">
        <Skeleton className="size-10 shrink-0 rounded-full" />
        <div className="grid gap-2">
          <Skeleton className="h-4 w-37.5" />
          <Skeleton className="h-4 w-25" />
        </div>
      </div>
    );
  }

  const user = data?.user;
  if (!user) {
    return (
      <div>
        <Button asChild>
          <Link href={"/sign-up"}>Sign up</Link>
        </Button>
      </div>
    );
  }
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button className="flex flex-row gap-3 p-6" variant="outline">
          <Avatar className="self-center">
            <AvatarImage src={data?.user?.image ?? ""} alt="avatar" />
            <AvatarFallback>?</AvatarFallback>
          </Avatar>
          <div className="flex flex-col">
            <p>Welcome Back, </p>
            <p>{user.name}</p>
          </div>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent>
        <DropdownMenuGroup>
          <DropdownMenuGroup>
            <DropdownMenuItem>
              <CircleUserRound /> Account
            </DropdownMenuItem>
          </DropdownMenuGroup>
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleSignOut} disabled={isSignOutPending}>
          <LogOutIcon />
          Sign Out
          {isSignOutPending && <Spinner />}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};

export default UserProfile;
