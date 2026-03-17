import { Skeleton } from "@/components/ui/skeleton";

export function SkeletonTable() {
  return (
    <div className="flex flex-col gap-2 m-4">
      {Array.from({ length: 5 }).map((_, index) => (
        <div className="flex gap-4" key={index}>
          <Skeleton className="h-24 flex-1" />
          <Skeleton className="h-24 flex-3" />
          <Skeleton className="h-24 flex-1" />
        </div>
      ))}
    </div>
  );
}
