import { Card } from "@/components/ui/card";
import { PropsWithChildren } from "react";

const SectionCard = ({ children }: PropsWithChildren) => {
  return <Card className="p-4 my-5">{children}</Card>;
};

export default SectionCard;
