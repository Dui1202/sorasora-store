import { SidebarProvider } from "@/components/ui/sidebar";
import NavBar from "./nav-bar";
import NavSidebar from "./nav-sidebar";

export default async function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
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
