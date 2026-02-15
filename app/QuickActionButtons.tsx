import { Button } from "@/components/ui/button";
import {
  NavigationMenuItem,
  NavigationMenuTrigger,
} from "@/components/ui/navigation-menu";
import { Bell, CirclePlus } from "lucide-react";
import React from "react";

const QuickActionButtons = () => {
  const buttons: { title: string; icon: React.ReactElement }[] = [
    { title: "Add new Entry", icon: <CirclePlus /> },
    { title: "Notification", icon: <Bell /> },
  ];
  return (
    <div className="flex gap-2">
      {buttons.map((item) => {
        return (
          <Button className="rounded-full" variant="secondary">
            {item.icon}
          </Button>
        );
      })}
    </div>
  );
};

export default QuickActionButtons;
