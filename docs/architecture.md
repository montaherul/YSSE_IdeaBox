# Architecture

YSSE Idea Box is a two-tier application: an ASP.NET Core Web API backend following **Clean Architecture**, and a React single-page application. This document describes the layering, the request lifecycle, and the frontend's feature-based structure.

---

## 1. System overview

```text
┌───────────────────────────────────────────────────────────────┐
│  React SPA (Vite)                                             │
│  TanStack Query · Zustand · React Hook Form + Zod · Axios     │
│  Radix UI · Tailwind CSS                                      │
└───────────────┬───────────────────────────────┬───────────────┘
                │ HTTPS / JSON                   │ WebSocket (SignalR)
                ▼                                ▼
┌───────────────────────────────────────────────────────────────┐
│  ASP.NET Core 8 Web API                                       │
│  Controllers → Application services → Infrastructure          │
│  Middleware: correlation, exception, logging, CORS,           │
│              rate limiting, timeouts, authN, authZ            │
└───────────────┬───────────────────────────────┬───────────────┘
                │ EF Core / Dapper               │ SMTP / HTTP
                ▼                                ▼
        ┌───────────────┐                ┌──────────────────┐
        │  PostgreSQL   │                │ Email / SMS /    │
        │               │                │ File storage     │
        └───────────────┘                └──────────────────┘
```

---

## 2. Backend — Clean Architecture

The solution (`backend/YSSE.IdeaBox.slnx`) contains four projects plus a test project. The dependency rule is enforced by project references:

```text
        ┌────────────────┐
        │     Domain     │   entities, enums, base types (no framework deps)
        └───────▲────────┘
                │
        ┌───────┴────────┐
        │  Application   │   services, DTOs, validators, abstractions
        └───────▲────────┘
                │
        ┌───────┴────────┐        ┌────────────────┐
        │ Infrastructure │◄───────│      Api       │
        └───────▲────────┘        └───────┬────────┘
                │                         │
        ┌───────┴─────────────────────────┴────────┐
        │            Tests (xUnit)                 │
        └──────────────────────────────────────────┘
```

| Project | Responsibility | Depends on |
| --- | --- | --- |
| `YSSE.IdeaBox.Domain` | Entities, enums, `BaseEntity`, `ISoftDeletable`. No EF/ASP.NET. | — |
| `YSSE.IdeaBox.Application` | Application services and their interfaces, DTOs, FluentValidation validators, `Result`/`PagedResult`, permission catalog, domain-support helpers. | Domain |
| `YSSE.IdeaBox.Infrastructure` | EF Core `AppDbContext`, entity configurations, generic repository + unit of work, migrations, Dapper queries, integrations (JWT, BCrypt, SMTP, SMS, storage, notifications). | Application, Domain |
| `YSSE.IdeaBox.Api` | Controllers, request pipeline, authentication/authorization, SignalR hub, DI composition. | Application, Infrastructure |
| `YSSE.IdeaBox.Tests` | xUnit unit/integration tests (in-memory EF Core). | all four |

Layers only depend inward. The Application layer never references EF Core types in its abstractions beyond the shared `IAppDbContext`/repository contracts; the Api layer never touches the database directly.

### Why this shape
- Business rules live in **Application services**, not controllers.
- Persistence is behind **abstractions** (`IUnitOfWork`, `IGenericRepository<T>`, `IAppDbContext`, query interfaces), so the application layer is testable and EF Core is swappable.
- The Api layer is a thin HTTP boundary: bind, validate, authorize, delegate, map to a response envelope.

---

## 3. Backend request lifecycle

Every request flows through the pipeline defined in `backend/src/YSSE.IdeaBox.Api/Program.cs`:

```text
HTTP request
   │
   ▼
Kestrel (15 MB max body / multipart limit)
   │
   ▼
1  CorrelationIdMiddleware        assign/propagate X-Correlation-ID + traceId
2  GlobalExceptionMiddleware      map exceptions → ApiErrorResponse
3  Serilog request logging
4  Swagger (dev) / HttpsRedirection (non-dev)
5  UseCors("spa")
6  UseRateLimiter                 global 120/min + named policies
7  UseRequestTimeouts             default 30s
8  UseAuthentication              validate JWT bearer token
9  UseAuthorization               role policies + permission policies
   │
   ▼
Endpoint routing → Controller action
   │
   ▼
Model binding + InvalidModelStateResponseFactory (400 VALIDATION envelope)
   │
   ▼
FluentValidationFilter           request DTO validators
   │
   ▼
Controller  →  Application service(s)  →  IUnitOfWork / EF Core / Dapper  →  PostgreSQL
   │
   ▼
Response envelope (ApiResponse / PagedApiResponse / ApiErrorResponse)
```

Middleware order is significant: correlation and exception handling are outermost so everything downstream is traced and any thrown `AppException` is converted to a consistent error response.

### Response envelope

| Kind | Shape |
| --- | --- |
| Single | `{ "success": true, "data": { … }, "traceId": "…" }` |
| Paged | `{ "success": true, "data": { "items": [ … ], "page": 1, "pageSize": 20, "total": 245, "hasNextPage": true }, "traceId": "…" }` |
| Error | `{ "success": false, "error": { "message": "…", "code": "VALIDATION", "details": { … } }, "traceId": "…" }` |

Full details are in [docs/api-reference.md](api-reference.md).

### Error handling
- `AppException` subclasses (`NotFoundException` 404, `ForbiddenException` 403, `UnauthorizedException` 401, `ConflictException` 409, `ValidationAppException` 400) are mapped by `GlobalExceptionMiddleware` to the error envelope with a machine-readable `code`.
- Unhandled exceptions become `500 INTERNAL_ERROR` with a generic message (no stack traces leak).
- Model-binding and FluentValidation failures both produce `400 VALIDATION`.

### Rate limiting, health, and observability
- Fixed-window rate limits: global 120/min; named `auth` 10/min, `guest-review` 3/hour, `write` 30/min, `messaging` 60/min. Rejections return `429` with the error envelope.
- Request timeouts: 30s default policy.
- Health checks: `/health/live` (liveness) and `/health/ready` (checks the database via `DatabaseHealthCheck`).
- Structured logging via Serilog with correlation IDs from `CorrelationIdMiddleware`.

---

## 4. Authentication & authorization at the boundary

- **Authentication** is JWT bearer (HS256). Access tokens carry `sub`, `jti`, `name`, `email`, and one `role` claim per role; refresh tokens are separate JWTs (`typ=refresh`) whose hashes are stored server-side for revocation.
- **Authorization** is policy-based:
  - Named role policies: `Admin`, `Moderator`, `ScreeningReviewer`, `OpportunitySeeker`.
  - **Permission policies** are synthesized on demand by `PermissionPolicyProvider` from any code in the permission catalog, and evaluated by `PermissionAuthorizationHandler`, which resolves the caller's permissions **from the database per request** (`IUserPermissionService`). Role/permission changes therefore take effect immediately without waiting for token expiry.
- Controllers are `[Authorize]` by default via `ApiControllerBase`; anonymous endpoints opt out with `[AllowAnonymous]`.

See [docs/security.md](security.md) for the full model.

---

## 5. Data access

- **EF Core** is the primary ORM (`AppDbContext`, snake_case naming, entity configurations, migrations).
- **Generic repository + unit of work** provide `Query()`, `GetByIdAsync`, `AddAsync`, etc., and `SaveChangesAsync`/transactions while sharing the scoped `DbContext`.
- **Dapper** is used for a small number of analytical reads where set-based SQL is clearer/more efficient (leaderboards, dashboard time series) via `ISqlQueryExecutor` and the `Queries/` implementations.

Details in [docs/database.md](database.md).

---

## 6. Frontend architecture

### Structure

```text
frontend/src/
├── app/            router.jsx, providers.jsx, query-client.js
├── layouts/        PublicLayout, WorkspaceLayout, ProtectedRoute/AdminRoute, Sidebar, Topbar
├── features/       one folder per domain module (pages, components, api.js, hooks.js)
├── components/     ui/ (primitives) + shared/ (composites) + errors/
├── services/       apiClient.js (axios instance + envelope unwrapping)
├── hooks/          useAuth, usePermission, useFormat, useNotificationHub, …
├── store/          Zustand stores: auth, ui, locale
├── navigation/     navigation.config.js (permission/role-driven sidebar)
├── config/         constants.js (API base URL, app name)
└── lib/            format.js, validation.js, sanitize.js, media.js, table.js
```

### Principles
- **Feature-based, not type-based.** Each domain module owns its pages, components, API calls, and hooks.
- **Server state in TanStack Query.** Caching, retries, and invalidation are handled by query keys per feature. Client-only state (auth, theme, locale) lives in Zustand.
- **One HTTP client.** All requests go through `services/apiClient.js`, which attaches the bearer token, unwraps the response envelope, normalizes paging, and transparently refreshes expired sessions on `401`.
- **Guard by roles *and* permissions.** Routes and navigation declare `roles` and/or `permissions`; `canAccess({ roles, permissions })` is a union check. UI gating is for UX only — the API is authoritative.

See [docs/frontend.md](frontend.md) for the routing table and per-feature breakdown.

---

## 7. Cross-cutting concerns

| Concern | Where |
| --- | --- |
| Validation | FluentValidation validators (backend) + Zod schemas (frontend); both enforced server-side. |
| Pagination | `Pagination.Normalize` (default 20, max 100) server-side; `PagedResult` envelope. |
| Soft delete | `ISoftDeletable` on user-facing content; queries filter `IsDeleted`. |
| Audit | `IAuditWriter` records significant administrative/state changes to `AuditLog`. |
| Notifications | `INotificationDispatcher` persists and pushes via SignalR; honors preferences. |
| File uploads | `IFileStorage` with size/type limits; served through `MediaController`. |
| Real-time | SignalR `NotificationHub`, optionally Redis-backed for multi-instance. |

---

## See also

- [Backend](backend.md) · [Frontend](frontend.md) · [Database](database.md)
- [API Reference](api-reference.md) · [Security](security.md) · [Testing & CI](testing-and-ci.md)
