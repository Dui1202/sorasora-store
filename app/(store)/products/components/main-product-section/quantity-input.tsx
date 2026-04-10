"use client";

import { type ChangeEvent, useEffect, useState } from "react";
import { Minus, Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

/**
 * Props for the quantity selector used on the product detail page.
 * Supports both uncontrolled usage via `defaultValue` and controlled usage via `value`.
 */
interface QuantityInputProps {
  className?: string;
  defaultValue?: number;
  value?: number;
  min?: number;
  max?: number;
  step?: number;
  disabled?: boolean;
  id?: string;
  name?: string;
  onValueChange?: (value: number) => void;
}

/**
 * Ensures the quantity always stays within the allowed range.
 */
const clampQuantity = (value: number, min: number, max?: number): number => {
  if (Number.isNaN(value)) {
    return min;
  }

  if (typeof max === "number") {
    return Math.min(Math.max(value, min), max);
  }

  return Math.max(value, min);
};

const getInitialQuantity = (
  value: number | undefined,
  defaultValue: number | undefined,
  min: number,
  max?: number,
): number => {
  return clampQuantity(value ?? defaultValue ?? min, min, max);
};

/**
 * Quantity input with increment/decrement controls.
 * Invalid or empty user input is normalized back to a safe numeric value.
 */
const QuantityInput = ({
  className,
  defaultValue = 1,
  value,
  min = 1,
  max,
  step = 1,
  disabled = false,
  id,
  name,
  onValueChange,
}: QuantityInputProps) => {
  const initialQuantity = getInitialQuantity(value, defaultValue, min, max);
  const isControlled = typeof value === "number";
  const [internalQuantity, setInternalQuantity] = useState(initialQuantity);
  const [inputValue, setInputValue] = useState(String(initialQuantity));

  const quantity = isControlled
    ? clampQuantity(value, min, max)
    : internalQuantity;

  useEffect(() => {
    setInputValue(String(quantity));
  }, [quantity]);

  const updateQuantity = (nextQuantity: number) => {
    const clampedQuantity = clampQuantity(nextQuantity, min, max);

    if (!isControlled) {
      setInternalQuantity(clampedQuantity);
    }

    setInputValue(String(clampedQuantity));
    onValueChange?.(clampedQuantity);
  };

  const handleDecrease = () => {
    updateQuantity(quantity - step);
  };

  const handleIncrease = () => {
    updateQuantity(quantity + step);
  };

  const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
    const nextValue = event.target.value;

    if (nextValue === "") {
      // Allow a temporary empty state while the user edits the field.
      setInputValue("");
      return;
    }

    if (!/^\d+$/.test(nextValue)) {
      return;
    }

    setInputValue(nextValue);
    updateQuantity(Number(nextValue));
  };

  const handleBlur = () => {
    if (inputValue === "") {
      updateQuantity(min);
      return;
    }

    updateQuantity(Number(inputValue));
  };

  const cannotDecrease = disabled || quantity <= min;
  const cannotIncrease = disabled || (typeof max === "number" && quantity >= max);

  return (
    <div className={cn("flex items-center gap-2", className)}>
      <Button
        type="button"
        variant="outline"
        size="icon"
        onClick={handleDecrease}
        disabled={cannotDecrease}
        aria-label="Decrease quantity"
      >
        <Minus />
      </Button>
      <Input
        id={id}
        name={name}
        type="number"
        inputMode="numeric"
        min={min}
        max={max}
        step={step}
        value={inputValue}
        onChange={handleInputChange}
        onBlur={handleBlur}
        disabled={disabled}
        className="h-9 w-16 text-center [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none"
        aria-label="Quantity"
      />
      <Button
        type="button"
        variant="outline"
        size="icon"
        onClick={handleIncrease}
        disabled={cannotIncrease}
        aria-label="Increase quantity"
      >
        <Plus />
      </Button>
    </div>
  );
};

export default QuantityInput;
