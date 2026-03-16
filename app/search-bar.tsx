import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group";
import { Search } from "lucide-react";
import React from "react";

const SearchBar = () => {
  return (
    <InputGroup className="max-w-xl">
      <InputGroupInput placeholder="Search..." />
      <InputGroupAddon>
        <Search />
      </InputGroupAddon>
      <InputGroupButton className="mr-2" variant="secondary">
        Search
      </InputGroupButton>
    </InputGroup>
  );
};

export default SearchBar;
