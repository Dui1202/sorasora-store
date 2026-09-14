// Presentation model; a future API adapter must map backend DTOs to this shape.
export type ProductListItem = {
  id: string;
  name: string;
  isPublished: boolean;
  productImages: Array<{ path: string; alt?: string | null }>;
  variants: Array<{
    id: string;
    name: string;
    sku: string;
    costPrice: number;
    sellingPrice: number;
    image: { path: string; alt?: string | null } | null;
  }>;
};
