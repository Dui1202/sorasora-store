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
  LayoutDashboard,
  ScrollText,
  Settings,
  ShoppingBag,
  UsersRound,
} from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import React from "react";
import Image from "next/image";
import logo from "@/public/logo.png";
type LinkObject = {
  title: string;
  href: string;
  icon: React.ReactElement;
};
const NavSideBar = () => {
  return (
    <Container>
      <NavLinks />
    </Container>
  );
};

const NavLinks = () => {
  const pathName = usePathname();
  const generalLinks: LinkObject[] = [
    { title: "Dashboard", href: "/", icon: <LayoutDashboard /> },
    { title: "Products", href: "/products", icon: <ShoppingBag /> },
    { title: "Orders", href: "/orders", icon: <ScrollText /> },
    { title: "Customers", href: "/customers", icon: <UsersRound /> },
  ];
  const supportLinks: LinkObject[] = [
    { title: "Settings", href: "/settings", icon: <Settings /> },
    { title: "Help", href: "/help", icon: <CircleQuestionMark /> },
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
              const isActive = pathName === item.href;
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
              const isActive = pathName === item.href;
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

export default NavSideBar;
