import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuList,
  NavigationMenuTrigger,
} from "@/components/ui/navigation-menu";
import { Container } from "@radix-ui/themes";
import React from "react";
import SearchBar from "./search-bar";
import QuickActionButtons from "./quick-action-buttons";
import UserProfile from "./user-profile";
import { Separator } from "@/components/ui/separator";

const NavBar = () => {
  return (
    <div className="w-full flex h-auto py-2 px-4 justify-between gap-3 border-b-2 sticky top-0 z-50 bg-background ">
      <SearchBar />
      <div className="flex flex-row gap-3 items-center">
        <QuickActionButtons />
        <Separator orientation="vertical" className="self-stretch" />
        <UserProfile />
      </div>
    </div>
  );
};

export default NavBar;
