import React from "react";
import Image from "next/image";
import logo from "@/public/logo.png";

const BrandLogo = () => {
  return (
    <Image
      src={logo}
      alt="Sora Sora Store Logo"
      width="1413"
      height="860"
      className="h-20 w-auto max-w-none shrink-0"
    />
  );
};

export default BrandLogo;
