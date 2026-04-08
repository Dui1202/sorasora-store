export type CartOwnerContext =
  | { userId: string; guestCartToken?: undefined }
  | { userId?: undefined; guestCartToken: string }
  | null;
