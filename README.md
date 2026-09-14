# Sora Sora Store

Next.js owns presentation and frontend form state. The ASP.NET Core modular
monolith in `backend/` owns authentication, persistence, inventory, and media.
The legacy Next.js backend has been removed; frontend integration with .NET is
still pending.

## Frontend development

```bash
npm ci
npm run dev
```

Open http://localhost:3000. No database connection, Better Auth secret, or storage
credentials are required to render the frontend. There is no Prisma client
generation step. The configured Google Fonts require network access during a build.

## Current UI availability

The admin and storefront layouts, product table, product forms, variation editor,
image previews, authentication forms, and warehouse form are preserved.

- The catalog and product list show an unavailable notice instead of live data.
- Product creation, sign-in, registration, and warehouse creation retain disabled
  forms. Product editing cannot load an existing product yet.
- Product table mutations are disabled. No frontend code calls the removed APIs.
- The admin layout is an unauthenticated UI shell with no live privileged data.
  Backend authorization and frontend session handling must be integrated before
  restoring live admin reads or writes.
- Existing order and product-detail mockups remain prototypes, not live commerce
  workflows.

## Removed backend

Removed `app/api` handlers, Better Auth/session-cookie middleware, Prisma
repositories and services, cart/order backend workflows, multipart server parsing,
storage SDK clients and upload/delete helpers, backend error adapters, and their
obsolete tests. Backend-only npm packages and Prisma generation configuration
were also removed.

Historical SQL files in `prisma/migrations/` remain untouched as migration history.
They are not used by the frontend. No database migration was generated or applied,
and no database data was changed.

## Reconnecting the frontend

1. Integrate .NET authentication/session handling and restore backend authorization
   (the backend currently contains a manual-testing anonymous-access bypass).
2. Define explicit product list and edit DTOs with images, variations, prices,
   stock, and concurrency versions; the existing .NET `ProductResponse` does not
   yet contain everything the editor needs.
3. Map those DTOs to the preserved frontend models. `ProductListItem` is a display
   model, not an existing .NET response. Product forms accept an `onSubmit`
   callback and are disabled when it is absent.
4. Implement .NET multipart request mappings, PUT updates, version-aware deletes,
   and ProblemDetails handling. The removed JSON `payload` multipart encoding is
   not a supported .NET contract.
5. Connect warehouse/franchise lookups and decide how product creation coordinates
   with inventory. `SpecificationFieldGroup` accepts lookup data as props.
6. Connect public catalog, product detail, cart, and order workflows to backend APIs.

Image display accepts absolute URLs. For relative object paths, optionally set
`NEXT_PUBLIC_PRODUCT_IMAGE_BASE_URL` to a public asset base URL. This only resolves
display URLs; it grants no storage access and requires no key.

Frontend supplier-country and order-status types are independent of persistence.
Form validation, variant draft logic, and their retained tests remain in place.

## Verification

```bash
npm run test -- --run
npx next typegen
npx tsc --noEmit --incremental false
npm run lint
npm run build
```
