import { Button } from "@/components/ui/button";
import { routes } from "@/lib/routes";
import Link from "next/link";

export default function UserProfile() {
  return (
    <Button asChild variant="outline">
      <Link href={routes.auth.signIn}>Sign in</Link>
    </Button>
  );
}
