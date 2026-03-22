# Overview

Sora Sora Store is an ecommerce system with two main parts:

1. **Store Management (Admin System)**
   - Used internally to manage products, variants, pricing, and inventory
   - Handles product creation, updates, and image uploads
   - Focuses on data integrity, type safety, and maintainability

2. **Web Store (Customer-Facing)**
   - Public storefront where customers browse and purchase products
   - Focuses on performance, user experience, and accurate product display

---

## Core Domain

The core domain of the system is **product management**, including:

- Products and variants
- Pricing and availability
- Product images (existing URLs + new uploads)

---

## Development Focus

Most development is currently focused on the **Store Management system**, especially:

- Product form handling
- Type-safe data mapping between backend and frontend
- Image upload and processing

# Tech Stack

## Core Technologies

- **Next.js (App Router)**
  - Used for both Store Management and Web Store
  - Prefer server components and server actions where appropriate
  - Avoid unnecessary client-side state

- **TypeScript (Strict Mode)**
  - All code must be fully type-safe
  - Do not use `any`
  - Prefer explicit types for complex logic

---

## Forms & Validation

- **React Hook Form**
  - Used for all forms
  - Do not manage form state manually

- **Zod**
  - Used for validation on both frontend and backend
  - Zod schemas are the single source of truth
  - Always use `zodResolver`

---

## Data Fetching

- **Axios**
  - Used for all HTTP requests
  - Do not use native `fetch` in client components
  - Centralize API calls in a dedicated layer

- **TanStack Query**
  - Used for client-side data fetching and caching
  - Prefer over manual `useEffect` fetching
  - Use mutations for writes and handle cache updates properly

- Prefer **server components** for data fetching when possible

- Use TanStack Query only when client-side interactivity is required

---

## Database & Backend

- **Prisma ORM**
  - Used for all database access
  - Must go through a service layer
  - Never access Prisma directly in UI or route handlers

- **PostgreSQL**
  - Primary database
  - Data integrity is critical

- **Supabase**
  - Used for external services (e.g. storage)
  - Keep usage isolated and explicit

---

## UI System

- **shadcn/ui + Radix UI**
  - Primary component system
  - Prefer composing existing components instead of building new ones

- **Tailwind CSS**
  - Used for styling
  - Use utility-first approach

- **class-variance-authority (CVA)**
  - Used for component variants

- **clsx + tailwind-merge**
  - Used for class composition
  - Avoid manual string concatenation

---

## Interaction & UX

- **dnd-kit**
  - Used for drag-and-drop (e.g. images, variants)
  - Follow existing patterns

- **motion**
  - Used for animations
  - Keep animations subtle and performant

---

## Key Principles

- Prefer server-side logic over client-side when possible
- Do not mix multiple data fetching strategies unnecessarily
- Reuse existing utilities and patterns before introducing new ones
- Maintain consistency across the codebase

# Architecture

Sora Sora Store follows a **feature-first modular monolith** with strict layer boundaries and server-first execution.

---

## Architectural Style

- Use **Next.js App Router** as the application shell.
- Organize by **feature/domain** (`product`, `variant`, `warehouse`), not by technical type alone.
- Each feature encapsulates:
  - types
  - validation schemas
  - business logic
  - mapping functions

- Keep business logic in reusable modules; UI and routes stay thin.

---

## Layers and Responsibilities

### Presentation Layer

(`app/*`, `features/*`, `components/*`)

- UI rendering, form wiring, interaction handling
- Must not contain business logic

---

### API Layer

(`app/api/*`)

- HTTP boundary only
- Handles:
  - request parsing
  - authentication / authorization
  - calling application services
  - response mapping

---

### Application Layer

(`lib/services/*` or feature-local services)

- Orchestrates use cases
- Coordinates:
  - database
  - storage
  - external services

---

### Domain Layer

(`lib/domain/*` or feature-local domain)

- Contains:
  - domain rules
  - invariants
  - pure logic (no side effects)

- Keep domain logic simple and pragmatic (avoid over-engineering)

---

### Infrastructure Layer

(`lib/infrastructure/*`)

- External integrations:
  - Prisma
  - Supabase
  - storage providers

- Use adapter pattern

---

### Contract Layer

(`lib/validation-schema/*`)

- Zod schemas as single source of truth
- Defines:
  - input validation
  - DTOs
  - form types

---

## Required Data Flow

1. UI collects input with React Hook Form
2. Zod validates input via `zodResolver`
3. API receives validated payload
4. API calls application service
5. Service executes use case with explicit mapping
6. API returns typed response

---

## Mapping Rules (CRITICAL)

- All transformations must use **explicit mapping functions**

Examples:

- `Product → UpdateProductFormInputData`
- `FormData → UpdateProductInput`

#### Rules

- Never pass database models directly to UI
- Never reuse backend types in forms
- Do not transform data inline inside components
- Keep mappings inside feature modules

---

## Server vs Client Responsibilities

### Server (Preferred)

- Data fetching
- Database operations
- Business logic

### Client

- UI interaction
- Form state
- Minimal logic only

---

## Non-Negotiable Rules

- No `any`; strict TypeScript everywhere
- No Prisma access from UI or route handlers
- No direct storage calls from UI
- Use Axios for client HTTP
- Use TanStack Query for client caching
- Prefer server components for read-heavy pages
- Keep side effects idempotent
- Standardize error handling with typed errors + single mapper
- Reuse existing schemas, mappers, and UI primitives

# Coding Rules

## TypeScript

- Use **strict TypeScript** at all times
- Never use `any`
- Prefer `unknown` over `any` when necessary
- Always define explicit types for:
  - function inputs
  - function outputs

- Avoid implicit `any` or overly inferred complex types

---

## Types & Data Modeling

- Do not reuse database (Prisma) types in the UI

- Do not reuse form types as backend input types

- Use **separate types per layer**:
  - Database types
  - API/DTO types
  - Form input types

- Types should be:
  - explicit
  - minimal
  - purpose-specific

---

## Functions

- Keep functions **small and focused**
- Each function should do **one thing only**
- Prefer **pure functions** (no side effects) when possible
- Avoid deeply nested logic

---

## Naming Conventions

- Use **descriptive and explicit names**

Examples:

- `mapProductToFormData` ✅

- `transformProduct` ❌

- Boolean variables:
  - prefix with `is`, `has`, `should`

- Functions:
  - use verbs (`createProduct`, `updateVariant`)

- Types:
  - use PascalCase

---

## Mapping & Transformation

- All transformations must use **explicit mapping functions**

- Never transform data inline inside components

- Mapping functions must:
  - be reusable
  - be type-safe
  - live in domain or mapper modules

---

## React Components

- Keep components **small and composable**

- Do not embed business logic inside components

- Avoid excessive props → use hooks or context

- Prefer:
  - server components for data-heavy pages
  - client components only when necessary

---

## Form Handling

- Always use React Hook Form

- Always use Zod resolver

- Do not manage form state manually

- Do not:
  - mix uncontrolled and controlled inputs incorrectly
  - duplicate validation logic outside Zod

---

## Error Handling

- Use **typed error objects**
- Do not throw raw strings
- Centralize error handling logic

---

## Code Reuse

- Reuse existing:
  - components
  - hooks
  - utilities
  - schemas

- Do not introduce new abstractions unless necessary

---

## Imports & Structure

- Use consistent import paths
- Avoid deeply nested relative imports when possible
- Group related logic together by feature

---

## Anti-Patterns (STRICT)

- ❌ Using `any`
- ❌ Inline complex transformations in UI
- ❌ Mixing business logic into components
- ❌ Duplicating schemas or types
- ❌ Writing large monolithic functions
- ❌ Creating unnecessary abstractions

# Patterns & Conventions

## Form Pattern

All forms must follow this structure:

1. Fetch initial data (server or query)
2. Map backend data → form input type
3. Initialize React Hook Form
4. Validate using Zod (`zodResolver`)
5. On submit:
   - Map form data → backend input type
   - Call API or server action

#### Rules

- Never pass backend data directly into forms
- Always use mapping functions
- Form types must be separate from backend types

---

## Mapping Pattern

All data transformations must use **explicit mapping functions**

### Naming

- `map<Entity>To<FormType>`
- `map<FormType>To<EntityInput>`

### Examples

- `mapProductToUpdateForm`
- `mapUpdateFormToProductInput`

### Rules

- Mapping must be:
  - pure
  - type-safe
  - reusable

- Do NOT:
  - transform data inline in components
  - mutate original objects

---

## API / Service Pattern

### Flow

```text
UI → API Route / Server Action → Service → Database
```

### Rules

- API routes:
  - handle HTTP concerns only
  - must not contain business logic

- Services:
  - contain all business logic
  - coordinate database and external calls

---

## Data Fetching Pattern

### Server-first approach

- Prefer data fetching in server components
- Use client fetching only when interactivity is required

### Client-side fetching

- Use TanStack Query
- Do not use `useEffect` for fetching

---

## Component Pattern

- Components must be:
  - small
  - reusable
  - focused

- Split components when:
  - logic becomes complex
  - UI becomes hard to read

---

## File Organization Pattern

Within each feature:

- group related files together
- keep logic close to usage

Example:

```text
features/product/
  ├── components/
  ├── hooks/
  ├── mappers/
  ├── schemas/
```

---

## Error Handling Pattern

- Use typed error objects
- Map errors to consistent API responses
- Do not throw raw strings

---

## Styling Pattern

- Use Tailwind for styling
- Use `clsx` + `tailwind-merge` for class composition
- Use `cva` for component variants

---

## Reuse First Principle

Before writing new code:

1. Check existing:
   - components
   - hooks
   - schemas
   - mappers

2. Extend existing patterns if possible

---

## Anti-Patterns (STRICT)

- ❌ Inline data transformation inside components
- ❌ Mixing multiple fetching strategies
- ❌ Business logic inside API routes
- ❌ Creating one-off patterns that break consistency
