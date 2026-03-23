# AGENTS.md

## Purpose

This file defines the engineering rules, architectural boundaries, and implementation conventions for **Sora Sora Store**.

It exists to keep the codebase:

- maintainable
- type-safe
- consistent
- easy to evolve

This project is currently a **feature-first modular monolith** built around a product-heavy ecommerce workflow.

---

## Project Context

Sora Sora Store has two main areas:

1. **Store Management (Admin System)**
   - Internal system for managing products, variants, pricing, inventory, and images
   - Prioritizes correctness, maintainability, and operational clarity

2. **Web Store (Customer-Facing)**
   - Public storefront for browsing and purchasing
   - Prioritizes performance, UX, and accurate data presentation

### Current Development Focus

Most active development is in the **Store Management** system, especially:

- product form workflows
- product and variant data mapping
- image upload and synchronization
- validation consistency between frontend and backend

### Core Domain

The current core domain is **product management**, including:

- products and variants
- pricing and availability
- product images (existing references and new uploads)

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

## 1. Strong Boundaries Between Layers

Business logic, transport logic, persistence logic, and UI logic must remain clearly separated.

## 2. Explicit Data Contracts

Data crossing a boundary must use explicit types and explicit mapping.

## 3. Server-First Design

Do work on the server by default. Only move logic to the client when interactivity requires it.

## 4. Type Safety Is Non-Negotiable

Use strict TypeScript. Do not use `any`.

## 5. Reuse Before Abstraction

Reuse existing patterns first. Add new abstractions only when repetition or complexity justifies them.

## 6. Consistency Beats Cleverness

Prefer boring, explicit, predictable code over compact or overly clever code.

---

# Architecture

## Architectural Style

Sora Sora Store uses a **feature-first modular monolith**.

Organize code primarily by **feature/domain**, not by technical type alone.

Examples:

- `product`
- `variant`
- `inventory`
- `warehouse`

Each feature should keep related logic close together where practical.

---

## Layers and Responsibilities

### Presentation Layer

**Examples:** `app/*`, `features/*`, `components/*`

Responsible for:

- rendering UI
- wiring forms
- handling user interaction
- displaying server or client data

Must not contain:

- persistence logic
- domain rules
- complex transformation logic

---

### API Layer

**Examples:** `app/api/*`, route handlers, server action boundaries

Responsible for:

- HTTP concerns
- request parsing
- authentication and authorization
- validation entry points
- calling application services
- mapping service results to responses

Must not contain:

- core business rules
- direct UI concerns
- ad hoc persistence orchestration beyond thin transport handling

---

### Application Layer

**Examples:** `lib/services/*` or feature-local services

Responsible for:

- use-case orchestration
- transaction boundaries
- coordination between database, storage, and external systems
- calling domain logic
- applying workflow rules

This is where most non-trivial business behavior should live.

---

### Domain Layer

**Examples:** `lib/domain/*` or feature-local domain modules

Responsible for:

- pure business rules
- invariants
- value-level validation that is not transport-specific
- deterministic logic without side effects

Do **not** introduce heavy DDD patterns unless the domain complexity actually requires them.

For this codebase, domain logic should remain pragmatic and lightweight.

---

### Infrastructure Layer

**Examples:** `lib/infrastructure/*`

Responsible for:

- Prisma access
- storage integrations
- Supabase integration
- external APIs
- adapters and gateways

Infrastructure should expose clear interfaces or functions that the application layer consumes.

---

### Contract Layer

**Examples:** `lib/validation-schema/*`, DTO modules, schema modules

Responsible for:

- boundary validation
- transport-level schemas
- DTO definitions
- form input/output contracts

Zod is the validation source of truth for boundary data, not the home of all business logic.

---

# Feature Boundaries

These rules matter more than folder names.

## Allowed Dependency Direction

Preferred dependency flow:

`Presentation -> API boundary -> Application -> Domain / Infrastructure`

## Boundary Rules

### Features Must Not Become Tightly Coupled

- Do not casually import internal implementation details from another feature.
- Cross-feature access should go through explicit public modules or shared abstractions.

### Shared Code Belongs in Shared Locations

Move logic to shared modules only when it is truly shared.

Good candidates:

- generic UI primitives
- shared validation utilities
- shared infrastructure clients
- common error utilities

Do not create a shared module just to avoid one duplicate function.

### Services Should Not Freely Call Other Services

Avoid service-to-service chains across unrelated domains.

If cross-feature coordination is needed:

- use a higher-level orchestrator
- or extract the shared behavior into a clear module

---

# Data and Type Modeling

## Separate Types by Boundary

Do not reuse one type everywhere.

Use separate types for:

- database models
- API DTOs
- form input/output
- domain-level structures when needed

## Rules

- Do not pass Prisma models directly into UI.
- Do not use form types as persistence input types.
- Do not leak infrastructure-specific shapes across layers.
- Keep types explicit and purpose-specific.

---

# Mapping Rules

All non-trivial transformations across boundaries must use **explicit mapping functions**.

## Required Mappings

Examples:

- `Product -> UpdateProductFormInputData`
- `UpdateProductFormData -> UpdateProductInput`
- `Prisma result -> API response DTO`

## Mapping Rules

- Keep mappings out of UI components.
- Keep mappings out of route handlers unless trivial.
- Prefer pure mapping functions.
- Do not mutate original objects.
- Name mappings explicitly.

### Good Names

- `mapProductToUpdateForm`
- `mapUpdateFormToProductInput`
- `mapProductRecordToDto`

### Bad Names

- `transformData`
- `normalizeThing`
- `prepareStuff`

---

# Server vs. Client Responsibilities

## Prefer Server-Side For

- data fetching
- persistence operations
- business logic
- authorization checks
- expensive computation

## Use Client-Side Only For

- interactive UI state
- form state
- drag-and-drop
- optimistic UX where justified
- client-only behavior that cannot run on the server

Client components should stay thin.

---

# Tech Guidance

These are current defaults, not eternal laws.

## Framework

- Use **Next.js App Router** as the application shell.
- Prefer server components for read-heavy pages.

## Language

- Use **TypeScript strict mode**.
- Do not use `any`.
- Prefer `unknown` when a value is truly unknown.

## Forms

- Use **React Hook Form** for forms.
- Use **Zod** for boundary validation.
- Use `zodResolver` when integrating Zod with React Hook Form.

## Data Fetching

- Prefer server-side data fetching where possible.
- For client-side async state and caching, use the project’s standard query/caching layer.
- Avoid ad hoc `useEffect` fetching when a structured data-fetching pattern exists.

## Persistence and Backend

- Use **Prisma** for database access.
- Prisma access should live in infrastructure or service layers, not UI.
- Use **PostgreSQL** as the primary database.

## External Services

- Keep **Supabase** or other external integrations isolated behind explicit modules.

## UI

- Prefer **pre-built components from shadcn/ui** as the default starting point.
- Only build custom components when:
  - a requirement cannot be satisfied by shadcn/ui
  - composition of existing components becomes overly complex or unclear
- Follow shadcn patterns and structure when extending components.
- Prefer composition over creating isolated one-off components.
- Keep styling consistent with Tailwind and existing design tokens.
- Avoid introducing new UI libraries unless there is a strong justification.

---

# Documentation Links

## Core

- Next.js: https://nextjs.org/docs
- TypeScript: https://www.typescriptlang.org/docs/

## Forms & Validation

- React Hook Form: https://react-hook-form.com/docs
- Zod: https://zod.dev/

## Data Fetching

- TanStack Query: https://tanstack.com/query/latest/docs/framework/react/overview
- Axios: https://axios-http.com/docs/intro

## Backend & Database

- Prisma: https://www.prisma.io/docs/orm
- PostgreSQL: https://www.postgresql.org/docs/current/index.html

## External Services

- Supabase: https://supabase.com/docs

## UI & Styling

- shadcn/ui: https://ui.shadcn.com/docs
- Radix UI: https://www.radix-ui.com/primitives/docs/overview/introduction
- Tailwind CSS: https://tailwindcss.com/docs
- class-variance-authority (CVA): https://cva.style/docs
- clsx: https://github.com/lukeed/clsx
- tailwind-merge: https://github.com/dcastil/tailwind-merge

---

# Coding Rules

## TypeScript

- Always declare clear function input and output types when the logic is non-trivial.
- Avoid overly clever inferred types when explicit types improve readability.
- Prefer narrow types over broad, permissive ones.

## Functions

- Keep functions focused.
- Prefer pure functions when possible.
- Avoid deep nesting.
- Extract branching logic when it improves clarity.

## React

- Keep components small and composable.
- Do not embed business logic in components.
- Do not perform complex transformations inline in JSX.
- Split components when readability drops.

## Naming

- Use descriptive names.
- Booleans should read like booleans: `is`, `has`, `should`, `can`.
- Functions should use verbs.
- Types should use PascalCase.

## Imports

- Use consistent import paths.
- Avoid deep relative import chains when the codebase already has path aliases.

---

# Form Pattern

All non-trivial forms should follow this flow:

1. Fetch initial data
2. Map backend data into a form input type
3. Initialize React Hook Form
4. Validate using Zod through `zodResolver`
5. On submit, map form data into an application input type
6. Call the API route or server action

## Form Rules

- Never feed raw database models directly into forms.
- Keep form types separate from backend input types.
- Do not duplicate validation logic in random components.
- Keep file handling and payload mapping explicit.

---

# API and Service Pattern

## Standard Flow

`UI -> API Route / Server Action -> Service -> Infrastructure`

## API Rules

- Handle transport concerns only.
- Parse input.
- Authenticate and authorize.
- Validate boundary input.
- Call the correct service.
- Map results to response shapes.

## Service Rules

- Own use-case orchestration.
- Coordinate database and storage work.
- Enforce workflow rules.
- Keep side effects explicit.

---

# Error Handling

Use a consistent typed error model.

## Requirements

- Do not throw raw strings.
- Prefer typed errors with stable machine-readable codes.
- Map internal errors to consistent API responses.
- Avoid leaking infrastructure details to the client.

## Minimum Structure

A typed application error should usually include:

- `code`
- `message`
- optional `details`
- optional `cause`

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

# Side Effects and Idempotency

Where practical:

- design writes to be idempotent
- make retries safe
- avoid partially completed workflows without compensation or clear cleanup

This matters especially for:

- image upload flows
- product updates with multiple related records
- external service coordination

---

# Performance and Scalability Guidance

Do not optimize blindly, but do not ignore obvious scale risks.

## Watch For

- N+1 queries
- overfetching large relational graphs
- unnecessary client-side fetching
- repeated mapping work in render paths
- oversized payloads for forms or product listings

## Prefer

- pagination for large collections
- explicit `select` / `include` usage in Prisma
- server-side aggregation when appropriate
- stable caching patterns where useful

---

# File Organization

Organize primarily by feature.

Example:

```text
features/product/
  components/
  hooks/
  mappers/
  schemas/
  services/
  domain/
```

This is a guideline, not a forced checklist. Only create folders that the feature actually needs.

---

# Testing Guidance

Test the logic where failure is costly.

## Prioritize Tests For

- mapping functions
- domain rules
- service workflows
- validation rules
- error mapping

## Lower Priority

- trivial presentational wrappers
- obvious pass-through code

Prefer a small number of useful tests over wide, shallow coverage.

---

# Enforcement

A rule that cannot be enforced will decay.

Use practical enforcement where possible:

- TypeScript strict mode
- ESLint rules
- import boundary rules
- code review checklists
- shared patterns and templates

## Minimum Enforcement Goals

- no `any`
- no Prisma in UI modules
- no complex inline transformations in components
- no cross-feature imports into private internals

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
- passing database models directly into UI
- inline complex data transformation inside components
- business logic inside route handlers
- direct storage access from UI
- one-off patterns that break established flow
- over-engineered domain models for simple CRUD cases
- creating shared modules prematurely

---

# Practical Defaults for This Codebase

When unsure, default to this:

- keep reads server-side
- keep client components minimal
- validate boundary input with Zod
- map explicitly across boundaries
- keep services responsible for workflows
- keep domain logic pure and small
- keep infrastructure isolated
- reuse existing patterns first

---

# Decision Rule

When making an implementation choice, prioritize in this order:

1. correctness
2. clarity
3. consistency with the existing architecture
4. maintainability
5. performance optimization
6. implementation speed

Do not sacrifice architectural correctness for minor short-term convenience.
