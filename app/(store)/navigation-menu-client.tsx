"use client";

import {
  NavigationMenuList,
  NavigationMenuItem,
  NavigationMenuTrigger,
  NavigationMenuContent,
  NavigationMenuLink,
  NavigationMenu,
} from "@/components/ui/navigation-menu";
import Link from "next/link";
import React from "react";

const NavigationMenuClient = () => {
  const navigationItems = [
    { name: "Home", href: "/" },
    {
      name: "Figures",
      href: "/products/figures",
      contentListItem: [
        {
          name: "Nendoroid",
          href: "/products/nendoroid",
        },
        {
          name: "Scale Figures",
          href: "/products/scale-figures",
        },
        {
          name: "Prize Figures",
          href: "/products/prize-figures",
        },
      ],
    },
    {
      name: "Other Products",
      href: "/products/other",
      contentListItem: [
        {
          name: "Keychains",
          href: "/products/keychains",
        },
        {
          name: "Posters",
          href: "/products/posters",
        },
        { name: "Plushies", href: "/products/plushies" },
        { name: "Other", href: "/products/other" },
      ],
    },
    { name: "Contact", href: "/contact" },
    { name: "News", href: "/news" },
    { name: "About", href: "/about" },
  ];
  return (
    <NavigationMenu>
      <NavigationMenuList className="gap-6">
        {navigationItems.map((item) => (
          <NavigationMenuItem key={item.name}>
            {item.contentListItem && (
              <>
                <NavigationMenuTrigger>{item.name}</NavigationMenuTrigger>
                <NavigationMenuContent>
                  <ul className="w-96">
                    {item.contentListItem.map((listItem) => (
                      <ListItem
                        key={listItem.name}
                        title={listItem.name}
                        href={listItem.href}
                      ></ListItem>
                    ))}
                  </ul>
                </NavigationMenuContent>
              </>
            )}

            {!item.contentListItem && (
              <NavigationMenuLink asChild>
                <Link href={item.href}>{item.name}</Link>
              </NavigationMenuLink>
            )}
          </NavigationMenuItem>
        ))}
      </NavigationMenuList>
    </NavigationMenu>
  );
};

function ListItem({
  title,
  children,
  href,
  ...props
}: React.ComponentPropsWithoutRef<"li"> & { href: string }) {
  return (
    <li {...props}>
      <NavigationMenuLink asChild>
        <Link href={href}>
          <div className="flex flex-col gap-1 text-sm">
            <div className="leading-none font-medium">{title}</div>
            <div className="line-clamp-2 text-muted-foreground">{children}</div>
          </div>
        </Link>
      </NavigationMenuLink>
    </li>
  );
}

export default NavigationMenuClient;
