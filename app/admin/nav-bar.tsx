import { Separator } from "@/components/ui/separator";
import QuickActionButtons from "./quick-action-buttons";
import SearchBar from "./search-bar";
import UserProfile from "./user-profile";

const NavBar = () => {
  return (
    <nav className="w-full flex h-auto py-2 px-4 justify-between gap-3 border-b-2 sticky top-0 z-50 bg-background ">
      <SearchBar />
      <div className="flex flex-row gap-3 items-center">
        <QuickActionButtons />
        <Separator orientation="vertical" className="self-stretch" />
        <UserProfile />
      </div>
    </nav>
  );
};

export default NavBar;
