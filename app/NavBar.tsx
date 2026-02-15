import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuList,
  NavigationMenuTrigger,
} from "@/components/ui/navigation-menu";
import { Container } from "@radix-ui/themes";
import React from "react";
import SearchBar from "./SearchBar";
import QuickActionButtons from "./QuickActionButtons";

const NavBar = () => {
  return (
    <div className="w-full flex h-auto py-2 px-4 justify-between gap-3 border-b-2">
      <SearchBar />
      <QuickActionButtons />
    </div>
  );
};

export default NavBar;
