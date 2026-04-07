import React from "react";
import NavBar from "./nav-bar";

const StoreLayout = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) => {
  return (
    <div>
      <NavBar />
      <main>{children}</main>
    </div>
  );
};

export default StoreLayout;
