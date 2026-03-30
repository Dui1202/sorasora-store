export const routes = {
  auth: {
    signIn: "/sign-in",
    signUp: "/sign-up",
  },
  admin: {
    root: "/admin",
    products: "/admin/products",
    productEdit: (id: string) => `/admin/products/${id}/edit`,
    orders: "/admin/orders",
    customers: "/admin/customers",
    shipping: "/admin/shipping",
    warehouses: "/admin/warehouses",
    warehouseNew: "/admin/warehouses/new",
    franchises: "/admin/franchises",
    settings: "/admin/settings",
    help: "/admin/help",
  },
} as const;
