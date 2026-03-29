"use client";

import { useEffect, useState } from "react";

export function useDelay(delay: number): boolean {
  const [isDelayed, setIsDelayed] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsDelayed(false);
    }, delay);

    return () => clearTimeout(timer);
  }, [delay]);

  return isDelayed;
}
