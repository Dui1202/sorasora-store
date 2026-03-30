"use client";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from "@/components/ui/sidebar";
import { Box, Container } from "@radix-ui/themes";
import {
  CircleQuestionMark,
  HeartPlus,
  LayoutDashboard,
  ScrollText,
  Settings,
  ShoppingBag,
  Truck,
  UsersRound,
  Warehouse,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import React from "react";
import Image from "next/image";
import logo from "@/public/logo.png";
import { routes } from "@/lib/routes";
type LinkObject = {
  title: string;
  href: string;
  icon: React.ReactElement;
};
const NavSidebar = () => {
  return (
    <Container>
      <NavLinks />
    </Container>
  );
};

const NavLinks = () => {
  const pathName = usePathname();
  const generalLinks: LinkObject[] = [
    { title: "Dashboard", href: routes.admin.root, icon: <LayoutDashboard /> },
    { title: "Products", href: routes.admin.products, icon: <ShoppingBag /> },
    { title: "Orders", href: routes.admin.orders, icon: <ScrollText /> },
    { title: "Customers", href: routes.admin.customers, icon: <UsersRound /> },
    { title: "Shipping", href: routes.admin.shipping, icon: <Truck /> },
    { title: "Warehouses", href: routes.admin.warehouses, icon: <Warehouse /> },
    { title: "Franchises", href: routes.admin.franchises, icon: <HeartPlus /> },
  ];
  const supportLinks: LinkObject[] = [
    { title: "Settings", href: routes.admin.settings, icon: <Settings /> },
    { title: "Help", href: routes.admin.help, icon: <CircleQuestionMark /> },
  ];

  return (
    <Sidebar>
      <SidebarHeader>
        <Box className="flex justify-center">
          <Image
            src={logo}
            alt="Sora Sora Store Logo"
            width="1413"
            height="860"
            className="h-20 w-auto max-w-none shrink-0"
          />
        </Box>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup className="border-t-2">
          <SidebarGroupLabel>General</SidebarGroupLabel>
          <SidebarMenu>
            {generalLinks.map((item) => {
              const isActive =
                pathName === item.href ||
                (item.href !== routes.admin.root &&
                  pathName.startsWith(`${item.href}/`));
              return (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={isActive}>
                    <Link href={item.href} className="flex items-center gap">
                      {item.icon} <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              );
            })}
          </SidebarMenu>
        </SidebarGroup>
        <SidebarGroup className="border-t-2">
          <SidebarGroupLabel>Support</SidebarGroupLabel>
          <SidebarMenu>
            {supportLinks.map((item) => {
              const isActive =
                pathName === item.href ||
                pathName.startsWith(`${item.href}/`);
              return (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={isActive}>
                    <Link href={item.href} className="flex items-center gap">
                      {item.icon} <span>{item.title}</span>
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              );
            })}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter></SidebarFooter>
    </Sidebar>
  );
};

export default NavSidebar;
