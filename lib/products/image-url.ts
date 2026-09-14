// Display-only URL resolution. Uploads and storage access belong to the backend.
// Future response mappers can supply absolute URLs directly. Relative object
// paths require a public asset base URL, never storage credentials.
export function getProductImageUrl(path: string): string {
  if (/^https?:\/\//.test(path) || path.startsWith("/")) {
    return path;
  }

  const baseUrl = process.env.NEXT_PUBLIC_PRODUCT_IMAGE_BASE_URL;
  if (!baseUrl) {
    return "";
  }

  const encodedPath = path.split("/").map(encodeURIComponent).join("/");
  return `${baseUrl.replace(/\/+$/, "")}/${encodedPath}`;
}
