import { Button } from "@/components/ui/button";
import { Bell, CirclePlus } from "lucide-react";
import React from "react";

const QuickActionButtons = () => {
  const buttons: { title: string; icon: React.ReactElement }[] = [
    { title: "Add new Entry", icon: <CirclePlus /> },
    { title: "Notification", icon: <Bell /> },
  ];
  return (
    <div className="flex gap-3">
      {buttons.map((item) => {
        return (
          <Button
            className="rounded-full"
            variant="secondary"
            size="icon"
            key={item.title}
          >
            {item.icon}
          </Button>
        );
      })}
    </div>
  );
};

export default QuickActionButtons;
