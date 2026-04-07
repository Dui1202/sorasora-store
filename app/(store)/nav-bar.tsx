import BrandLogo from "@/components/brand-logo";
import { Button } from "@/components/ui/button";
import { ShoppingCart } from "lucide-react";
import UserProfile from "../admin/user-profile";
import NavigationMenuClient from "./navigation-menu-client";
import SearchBar from "./search-bar";

const NavBar = () => {
  return (
    <nav className="w-full flex h-auto py-2 px-4 gap-3 border-b-2 sticky top-0 z-50 bg-background items-center justify-between">
      <div className="basis-1/4 justify-end flex">
        <BrandLogo />
      </div>
      <div className="flex flex-col items-center gap-3 basis-1/2">
        <SearchBar />
        <NavigationMenuClient />
      </div>
      <div className="flex gap-3  items-center basis-1/4">
        <Button variant="ghost">
          <ShoppingCart />
        </Button>
        <UserProfile />
      </div>
    </nav>
  );
};

export default NavBar;
