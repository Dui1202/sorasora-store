import { generateToken } from "../helper";

export function generateGuestCartToken() {
  const token = generateToken();
  return token;
}
