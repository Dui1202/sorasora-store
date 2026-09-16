import { Card } from "@/components/ui/card";

type SectionCardProps = {
  children: React.ReactNode;
  className?: string;
};

const SectionCard = ({ children, className }: SectionCardProps) => {
  return <Card className={`p-4 my-5 ${className ?? ""}`}>{children}</Card>;
};

export default SectionCard;
