# AGENTS.md

## Purpose

This file defines the engineering rules, architectural boundaries, and implementation conventions for **Sora Sora Store** during the migration from a Next.js-backed application to a **Next.js frontend + ASP.NET Core backend**.

It exists to keep the codebase:

- maintainable
- type-safe
- consistent
- easy to evolve

This project should remain a **feature-first modular monolith** even as the backend moves to .NET.

---

## Project Context

Sora Sora Store has two main areas:

1. **Store Management (Admin System)**
   - Internal system for managing products, variants, pricing, inventory, orders, and images
   - Prioritizes correctness, maintainability, operational clarity, and safe write workflows

2. **Web Store (Customer-Facing)**
   - Public storefront for browsing and purchasing
   - Prioritizes performance, UX, accurate data presentation, and stable read APIs

### Current Development Focus

The current focus is the backend migration to **ASP.NET Core**, especially:

- moving backend workflows out of Next route handlers
- establishing a .NET modular monolith with explicit boundaries
- migrating product and variant management first
- centralizing image upload and synchronization in the backend
- keeping validation and error contracts consistent during the transition

### Core Domain

The current core domain is still **product management**, including:

- products and variants
- pricing and availability
- warehouse inventory
- product images
- product form workflows and data mapping

Orders, carts, and storefront queries are important, but product workflows are still the highest-priority migration path.

---

# Rule Hierarchy

Not all guidance has the same weight.

## 1. Principles

Principles are hard constraints. Do not violate them unless the architecture is being intentionally changed.

## 2. Rules

Rules are the default way to build in this codebase. Break them only with a clear reason.

## 3. Conventions

Conventions are preferred patterns for consistency. They may be adapted when needed.

---

# Principles

## 1. Strong Boundaries Between Frontend and Backend

The frontend and backend are separate systems.

- **Next.js** is the presentation layer and frontend application shell
- **ASP.NET Core** is the backend authority for business workflows, data access, auth, and persistence

Do not blur those responsibilities.

## 2. Explicit Data Contracts

Data crossing a boundary must use explicit DTOs, request models, response models, or form models.

Do not reuse the same type everywhere just because it is convenient.

## 3. Server-First Design

Business logic, persistence, authorization, and integrations belong on the server by default.

Only keep logic in the client when interactivity requires it.

## 4. Type Safety Is Non-Negotiable

- Use strict TypeScript on the frontend
- Use nullable reference types and explicit typing in C#
- Do not use `any`
- Avoid `dynamic` unless there is a strong interoperability reason

## 5. Reuse Before Abstraction

Reuse existing patterns first. Add new abstractions only when repetition, coupling, or complexity justifies them.

## 6. Consistency Beats Cleverness

Prefer boring, explicit, predictable code over compact or overly clever code.

## 7. The Migration Must Reduce Dual Ownership

Do not build the same backend behavior twice unless it is a deliberate temporary bridge.

During migration:

- new backend logic should be added in .NET by default
- Next API routes may proxy temporarily, but should not become the long-term home of business logic
- Prisma-backed backend behavior should not expand unless needed for a temporary compatibility bridge

---

# Architecture

## Target Architecture

Sora Sora Store uses a **feature-first modular monolith** with:

- **Next.js App Router** for UI, SSR, and frontend composition
- **ASP.NET Core** for API endpoints and backend workflows
- **PostgreSQL** as the primary database
- **EF Core** as the primary backend ORM
- **Supabase or compatible object storage** behind backend-managed abstractions

## Architectural Style

Organize code primarily by **feature/domain**, not by technical type alone.

Examples:

- `Products`
- `Inventory`
- `Catalog`
- `Warehouses`
- `Orders`
- `Carts`
- `Auth`

Each feature should keep related application logic, domain logic, contracts, and infrastructure close together where practical.

## Backend Module Shape

Each .NET module should usually be organized around these responsibilities:

- `Contracts`
- `Application`
- `Domain`
- `Infrastructure`

This is a logical boundary first. Folder names may vary slightly, but the boundary must remain clear.

---

## Layers and Responsibilities

### Frontend Presentation Layer

**Examples:** `app/*`, `feature/*`, `components/*`, `hooks/*`

Responsible for:

- rendering UI
- wiring forms
- handling user interaction
- calling backend APIs
- mapping backend responses into view models or form input models

Must not contain:

- persistence logic
- backend business rules
- direct storage access
- long-lived auth logic beyond frontend session handling
- complex cross-boundary transformations inline in components

### Backend API Layer

**Examples:** ASP.NET Core route groups, endpoint handlers, filters, auth middleware

Responsible for:

- HTTP concerns
- request parsing
- authentication and authorization
- boundary validation
- calling application services
- mapping application results to response DTOs or `ProblemDetails`

Must not contain:

- database orchestration beyond thin transport handling
- business rules that belong in application or domain code
- UI concerns

### Application Layer

Responsible for:

- use-case orchestration
- transaction boundaries
- coordination between database, storage, and external systems
- workflow rules
- cross-aggregate consistency

This is where most non-trivial backend behavior should live.

### Domain Layer

Responsible for:

- business invariants
- pure domain rules
- value-level validation that is not transport-specific
- deterministic logic without side effects

Do **not** introduce heavy DDD ceremony unless the domain actually requires it.

For this codebase, domain logic should remain pragmatic and lightweight.

### Infrastructure Layer

Responsible for:

- EF Core persistence
- storage integrations
- Supabase integration
- external APIs
- auth adapters
- repository implementations when needed

Infrastructure should expose clear functions, services, or interfaces that the application layer consumes.

### Contract Layer

Responsible for:

- request DTOs
- response DTOs
- validation models
- form contracts
- machine-readable error contracts

Contracts define boundary shape. They are not the home of business logic.

---

# Feature Boundaries

These rules matter more than folder names.

## Allowed Dependency Direction

Preferred dependency flow:

`Frontend UI -> Backend API -> Application -> Domain / Infrastructure`

Inside the backend:

`API -> Application -> Domain / Infrastructure`

## Boundary Rules

### Features Must Not Become Tightly Coupled

- Do not casually import internal implementation details from another feature
- Cross-feature access should go through explicit public application contracts or shared abstractions
- Do not let one feature reach into another feature's EF internals or entity configuration

### Application Services Should Not Freely Chain Across Domains

Avoid service-to-service chains across unrelated domains.

If cross-feature coordination is needed:

- use a higher-level orchestrator
- or extract shared behavior into a dedicated module or shared service

### Shared Code Belongs in Shared Locations

Move logic to shared modules only when it is truly shared.

Good candidates:

- common error types
- common pagination models
- shared auth abstractions
- storage abstractions
- reusable frontend UI primitives

Do not create a shared module just to avoid one duplicate function.

---

# Data and Type Modeling

## Separate Types by Boundary

Do not reuse one type everywhere.

Use separate types for:

- EF Core entities
- domain models when needed
- request DTOs
- response DTOs
- frontend form input/output models
- frontend display models when needed

## Rules

- Do not pass EF entities directly into API responses
- Do not use form types as persistence input types
- Do not leak infrastructure-specific shapes across layers
- Keep types explicit and purpose-specific

## Database Model Organization

Do **not** store the database model in one giant file the way `schema.prisma` often centralizes schema.

Prefer:

- one entity class per table or aggregate root
- one EF Core configuration class per entity
- one shared `DbContext`
- organization by feature/module, not by one monolithic schema file

Typical shape:

```text
backend/
  SoraSora.Modules.Products/
    Domain/
      Product.cs
      ProductVariant.cs
    Infrastructure/
      Persistence/
        Configurations/
          ProductConfiguration.cs
          ProductVariantConfiguration.cs

  SoraSora.Infrastructure/
    Persistence/
      SoraSoraDbContext.cs
```

### Persistence Rules

- entity classes model persistence and backend behavior, not frontend contracts
- keep EF configuration out of the API layer
- use explicit configuration classes for indexes, keys, relationships, lengths, precision, enum conversions, and table names
- apply configurations through `DbContext`

---

# Mapping Rules

All non-trivial transformations across boundaries must use **explicit mapping functions**.

## Required Mappings

Examples:

- `ProductEntity -> ProductDto`
- `UpdateProductRequest -> UpdateProductCommand`
- `ProductDto -> UpdateProductFormInput`
- `FormData -> Multipart request DTO`

## Mapping Rules

- keep mappings out of React components
- keep mappings out of endpoint handlers unless trivial
- prefer pure mapping functions
- do not mutate original objects
- name mappings explicitly

### Good Names

- `MapProductToDto`
- `MapUpdateProductRequestToCommand`
- `mapProductDtoToUpdateForm`

### Bad Names

- `transformData`
- `normalizeThing`
- `prepareStuff`

---

# Frontend vs. Backend Responsibilities

## Prefer Frontend For

- rendering pages and components
- form state and interactive state
- optimistic UX where justified
- server-side rendering of backend-backed pages
- composing data from backend APIs for the UI

## Prefer Backend For

- data fetching rules
- persistence operations
- business logic
- authorization checks
- image upload and synchronization
- expensive computation
- transaction handling
- external service coordination

Client components should stay thin.

---

# Tech Guidance

These are current defaults, not eternal laws.

## Frontend

- Use **Next.js App Router** as the frontend application shell
- Prefer server components for read-heavy pages
- Use **TypeScript strict mode**
- Use **React Hook Form** for non-trivial forms
- Use **Zod** for frontend boundary validation and form schemas
- Use `zodResolver` with React Hook Form where appropriate
- Prefer the project's structured query/caching patterns over ad hoc `useEffect` fetching

## Backend

- Use **ASP.NET Core** for HTTP APIs
- Use **Minimal APIs** or route-group based endpoint organization by default
- Use **EF Core** for primary database access
- Use **FluentValidation** or equivalent boundary validation for backend requests
- Use **PostgreSQL** as the primary database
- Keep auth, persistence, and storage coordination on the backend

## External Services

- Keep **Supabase** or other storage integrations isolated behind explicit backend modules
- The frontend must not directly own storage writes for core admin workflows unless the architecture is intentionally changed

## UI

- Prefer **pre-built components from shadcn/ui** as the default starting point
- Only build custom components when:
  - a requirement cannot be satisfied by shadcn/ui
  - composition of existing components becomes overly complex or unclear
- Follow shadcn patterns and structure when extending components
- Prefer composition over isolated one-off components
- Keep styling consistent with Tailwind and existing design tokens

---

# Documentation Links

## Core

- ASP.NET Core: https://learn.microsoft.com/aspnet/core
- EF Core: https://learn.microsoft.com/ef/core
- Next.js: https://nextjs.org/docs
- TypeScript: https://www.typescriptlang.org/docs/

## Validation

- FluentValidation: https://docs.fluentvalidation.net
- React Hook Form: https://react-hook-form.com/docs
- Zod: https://zod.dev/

## Data and Database

- PostgreSQL: https://www.postgresql.org/docs/current/index.html
- Npgsql EF Core Provider: https://www.npgsql.org/efcore/

## External Services

- Supabase: https://supabase.com/docs

## UI and Styling

- shadcn/ui: https://ui.shadcn.com/docs
- Radix UI: https://www.radix-ui.com/primitives/docs/overview/introduction
- Tailwind CSS: https://tailwindcss.com/docs

---

# Coding Rules

## TypeScript

- always declare clear function input and output types when logic is non-trivial
- avoid overly clever inferred types when explicit types improve readability
- prefer narrow types over broad, permissive ones
- do not use `any`

## C#

- keep nullable reference types enabled
- prefer explicit request, response, and command models over anonymous shapes
- keep endpoint handlers thin
- keep application services focused
- do not return EF entities directly from endpoints
- avoid `dynamic` unless required by external interop

## Functions and Methods

- keep functions focused
- prefer pure functions when possible
- avoid deep nesting
- extract branching logic when it improves clarity

## React

- keep components small and composable
- do not embed business logic in components
- do not perform complex transformations inline in JSX
- split components when readability drops

## Naming

- use descriptive names
- booleans should read like booleans: `is`, `has`, `should`, `can`
- functions and methods should use verbs
- types should use PascalCase

---

# Frontend Form Pattern

All non-trivial forms should follow this flow:

1. Fetch initial data from the backend
2. Map backend data into a frontend form input type
3. Initialize React Hook Form
4. Validate with Zod through `zodResolver`
5. On submit, map form data into a backend request DTO
6. Call the backend API

## Form Rules

- never feed raw backend entities directly into forms
- keep form types separate from backend request types
- do not duplicate validation logic in random components
- keep file handling and payload mapping explicit

---

# Backend API and Application Pattern

## Standard Flow

`Frontend -> ASP.NET Core endpoint -> Application service -> Infrastructure`

## Endpoint Rules

- handle transport concerns only
- parse input
- authenticate and authorize
- validate boundary input
- call the correct application service
- map results to DTOs or `ProblemDetails`

## Application Service Rules

- own use-case orchestration
- coordinate database and storage work
- enforce workflow rules
- keep side effects explicit
- define transaction boundaries intentionally

## Persistence Rules

- use EF Core in infrastructure or persistence-focused modules
- avoid leaking `DbContext` into the frontend or endpoint composition code beyond justified thin orchestration
- avoid fat repositories that hide all query behavior; use repositories only where they clarify the boundary

---

# Error Handling

Use a consistent typed error model across the backend.

## Requirements

- do not throw raw strings
- prefer typed application errors with stable machine-readable codes
- map internal failures to consistent HTTP responses
- avoid leaking infrastructure details to the client

## HTTP Error Format

Use **RFC 7807 ProblemDetails** as the standard error envelope.

Include stable machine-readable codes that mirror the current TypeScript error model.

Typical fields:

- `type`
- `title`
- `status`
- `detail`
- `instance`
- `code`
- optional `errors`
- optional `details`

## Error Categories

Use consistent categories such as:

- validation error
- authentication error
- authorization error
- not found error
- conflict error
- external service error
- unexpected internal error

All API error responses should be shaped consistently.

---

# Auth and Security

## Auth Ownership

Authentication and authorization are backend responsibilities.

The target state is:

- auth is issued and enforced by .NET
- frontend consumes authenticated APIs
- roles are enforced in backend authorization policies or endpoint guards

## Rules

- do not put authorization decisions in React components
- use frontend guards for UX only, never as the real enforcement mechanism
- keep session and identity rules centralized
- treat admin endpoints and storefront endpoints as distinct security surfaces

---

# Side Effects and Idempotency

Where practical:

- design writes to be idempotent
- make retries safe
- avoid partially completed workflows without compensation or cleanup

This matters especially for:

- image upload flows
- product updates with multiple related records
- inventory movements
- order creation
- external storage coordination

---

# Performance and Scalability Guidance

Do not optimize blindly, but do not ignore obvious scale risks.

## Watch For

- N+1 queries
- overfetching large relational graphs
- unnecessary client-side fetching
- repeated mapping work in render paths
- oversized payloads for forms or listings

## Prefer

- pagination for large collections
- explicit query shapes in EF Core
- projections to lean DTOs
- backend-side aggregation where appropriate
- stable caching patterns where useful

---

# File Organization

Organize primarily by feature.

## Frontend Example

```text
feature/products/
  components/
  hooks/
  mappers/
  schemas/
```

## Backend Example

```text
backend/
  SoraSora.Modules.Products/
    Contracts/
    Application/
    Domain/
    Infrastructure/
```

This is a guideline, not a forced checklist. Only create folders that the feature actually needs.

---

# Testing Guidance

Only create or modify test scripts or test files when the user explicitly requests tests.
Running existing tests for verification is allowed when relevant.

Test the logic where failure is costly.

## Prioritize Tests For

- mapping functions
- domain rules
- application workflows
- validation rules
- error mapping
- inventory and image side effects

## Lower Priority

- trivial presentational wrappers
- obvious pass-through code

Prefer a small number of useful tests over wide, shallow coverage.

---

# Enforcement

A rule that cannot be enforced will decay.

Use practical enforcement where possible:

- TypeScript strict mode
- nullable reference types
- ESLint rules
- backend validation and error middleware
- code review checklists
- shared patterns and templates

## Minimum Enforcement Goals

- no `any`
- no `dynamic` without justification
- no EF entities in frontend code
- no business logic in React components
- no business logic in thin endpoint handlers
- no direct storage access from the frontend for core admin workflows
- no cross-feature imports into private internals

---

# Migration Rules

Because the codebase is in transition, these rules are especially important.

## Database Migration Ownership

- Codex must not create, edit, remove, regenerate, or apply database migration files
- Codex must not run schema-mutating migration commands such as `dotnet ef migrations add`, `dotnet ef database update`, or `prisma migrate`
- when a model or schema change requires a migration, make the requested source/configuration changes, report exactly what must be migrated, and leave migration generation and application to the user
- inspecting existing migrations and reporting migration requirements is allowed, but migration files must remain untouched unless the user explicitly requests otherwise

## Default Direction

- new backend features should be implemented in .NET first
- existing Next API routes may temporarily proxy or bridge to the backend
- do not introduce new permanent backend logic in `app/api/*` unless there is a clear migration reason

## Data Ownership

- do not treat Prisma as the long-term backend architecture
- avoid deepening Prisma-backed service layers when the same work belongs in EF Core
- when EF Core is established for a backend flow, future schema changes should follow EF Core migration practices for that flow

## Frontend Migration

- prefer calling the .NET backend directly or through a deliberate thin frontend integration layer
- keep frontend contracts explicit during cutover
- do not let temporary compatibility code become the new architecture by accident

---

# When to Introduce New Abstractions

Create a new abstraction only when at least one of these is true:

- the same logic is repeated meaningfully
- a boundary needs to be protected
- a concept is stable enough to deserve a name
- testing becomes easier and clearer with extraction

Do not abstract for aesthetics alone.

---

# Anti-Patterns

Avoid these unless there is a very strong reason:

- using `any`
- using `dynamic` casually
- passing EF entities directly into API responses or UI
- inline complex data transformation inside components
- business logic inside thin endpoint handlers
- direct storage access from the frontend
- creating a giant EF schema file equivalent to `schema.prisma`
- over-engineered domain models for simple CRUD cases
- one-off patterns that break established flow
- creating shared modules prematurely

---

# Practical Defaults for This Codebase

When unsure, default to this:

- keep Next focused on UI and frontend composition
- keep business workflows in .NET
- validate frontend input with Zod
- validate backend boundary input with backend validators
- map explicitly across boundaries
- keep application services responsible for workflows
- keep domain logic pure and small
- keep infrastructure isolated
- use EF Core as the backend persistence default
- use `ProblemDetails` plus stable error codes for API failures
- reuse existing patterns first

---

# Decision Rule

When making an implementation choice, prioritize in this order:

1. correctness
2. clarity
3. consistency with the target architecture
4. maintainability
5. performance optimization
6. implementation speed

Do not sacrifice architectural correctness for minor short-term convenience.
