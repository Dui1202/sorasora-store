import {
  InputGroup,
  InputGroupInput,
  InputGroupAddon,
  InputGroupButton,
} from "@/components/ui/input-group";
import { Search } from "lucide-react";
import React from "react";

const SearchBar = () => {
  return (
    <InputGroup className="max-w-xl self-center">
      <InputGroupInput placeholder="Search..." />
      <InputGroupButton className="mr-2" variant="secondary">
        <Search />
      </InputGroupButton>
    </InputGroup>
  );
};

export default SearchBar;
