import { UserRole } from "@/app/generated/prisma/enums";
import { SidebarProvider } from "@/components/ui/sidebar";
import { requireRole } from "@/lib/auth/authorization";
import NavBar from "./nav-bar";
import NavSidebar from "./nav-sidebar";

export default async function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  await requireRole([UserRole.STAFF, UserRole.ADMIN]);

  return (
    <SidebarProvider>
      <NavSidebar />
      <div className="flex-1 flex flex-col">
        <NavBar />
        <main>{children}</main>
      </div>
    </SidebarProvider>
  );
}
