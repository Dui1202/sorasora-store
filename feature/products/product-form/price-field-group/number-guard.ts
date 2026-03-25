import type { ClipboardEvent, KeyboardEvent } from "react";

const ALLOWED_CONTROL_KEYS = new Set([
  "Backspace",
  "Delete",
  "Tab",
  "Enter",
  "ArrowLeft",
  "ArrowRight",
  "ArrowUp",
  "ArrowDown",
  "Home",
  "End",
]);

export function toDigitsOnly(value: string) {
  return value.replace(/\D+/g, "");
}

export function handleDigitOnlyKeyDown(
  event: KeyboardEvent<HTMLInputElement>,
) {
  if (event.ctrlKey || event.metaKey || event.altKey) {
    return;
  }

  if (ALLOWED_CONTROL_KEYS.has(event.key) || /^\d$/.test(event.key)) {
    return;
  }

  event.preventDefault();
}

export function handleDigitOnlyPaste(
  event: ClipboardEvent<HTMLInputElement>,
) {
  const pastedText = event.clipboardData.getData("text");

  if (!/^\d*$/.test(pastedText)) {
    event.preventDefault();
  }
}
